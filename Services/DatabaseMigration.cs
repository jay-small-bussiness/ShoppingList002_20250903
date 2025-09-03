using ShoppingList002.Models.DbModels;
using ShoppingList002.Services.Converter;
using ShoppingList002.Services.Converters;
using System;
using System.Threading.Tasks;


namespace ShoppingList002.Services
{
    public class DatabaseMigration
    {
        private readonly IDatabaseService _databaseService;

        public DatabaseMigration(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task MigrateIfNeededAsync()
        {
            var currentVersion = await _databaseService.GetCurrentDbVersionAsync();

            while (true)
            {
                if (currentVersion == "1.0.0")
                {
                    await Migrate_1_0_0_to_1_0_1();
                    currentVersion = "1.0.1";
                    await _databaseService.SetVersionAsync(currentVersion);
                    continue;
                }

                // これ以降のバージョンアップが追加されたらここに追記

                break; // 最新状態ならループ終了
            }
            currentVersion = await _databaseService.GetCurrentDbVersionAsync();

            if (currentVersion == "1.0.1")
            {
                await Migrate_1_0_1_to_1_0_2();
                await _databaseService.SetVersionAsync("1.0.2");
            }
            currentVersion = await _databaseService.GetCurrentDbVersionAsync();
            if (currentVersion == "1.0.2")
            {
                await Migrate_1_0_2_to_1_1_0();
                await _databaseService.SetVersionAsync("1.1.0");
            }
            // 将来的にはここに追加されていく形：
            // if (currentVersion == "1.0.2") { await Migrate_1_0_2_to_2_0_0(); ... }
        }
        private async Task Migrate_1_0_0_to_1_0_1()
        {
            // AppSetting テーブルが存在しない前提で作成
            await _databaseService.CreateTableAsync<AppSettingDbModel>();

            // デフォルト設定追加
            var setting = new AppSettingDbModel
            {
                Key = "SoftDeleteRetentionDays",
                Value = "365",
                UpdatedDate = DateTimeOffset.Now
            };
            await _databaseService.InsertOrReplaceAsync(setting);
        }
        private async Task Migrate_1_0_1_to_1_0_2()
        {
            Console.WriteLine("🔧 マイグレーション開始: 1.0.1 → 1.0.2");

            await _databaseService.CreateTableAsync<ActivityLogDbModel>();

            Console.WriteLine("✅ マイグレーション完了: 1.0.2 に更新");
        }
        private async Task Migrate_1_0_2_to_1_1_0()
        {
            Console.WriteLine("🔧 マイグレーション開始: 1.0.2 → 1.1.0");

            var conn = await _databaseService.GetConnectionAsync();

            try
            {
                // カラム追加（既存なら落ちる→catchでスキップ）
                await conn.ExecuteAsync("ALTER TABLE CandidateListItem ADD COLUMN Kana TEXT");
                await conn.ExecuteAsync("ALTER TABLE CandidateListItem ADD COLUMN SearchKana TEXT");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ カラム追加スキップまたは失敗: {ex.Message}");
            }

            // 🔁 ここから先の補完は“今回決めた統一ロジック”で一括
            var fixedCount = await KanaFieldUpdater.RebuildAllAsync(_databaseService);
            Console.WriteLine($"🛠 Kana/SearchKana 再計算: {fixedCount} 件更新");

            Console.WriteLine("✅ マイグレーション完了: 1.1.0 に更新");
        }
    }

}

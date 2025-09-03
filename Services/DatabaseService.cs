using SQLite;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Microsoft.Maui.Storage;
using ShoppingList002.Models;
using ShoppingList002.Models.DbModels;
using ShoppingList002.Models.JsonModels;
using System.Linq.Expressions;
using System.Diagnostics;

namespace ShoppingList002.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly SQLiteAsyncConnection _connection;
        private readonly ISettingsService _settingsService;
        public DatabaseService(ISettingsService settingsService)
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "shoppinglist.db3");
            _connection = new SQLiteAsyncConnection(dbPath);
            _settingsService = settingsService;
        }

        public async Task DatabaseTest()
        {
            var oldDate = DateTimeOffset.Now.AddDays(-365); // 45日前

            var testItem = new ShoppingListItemDbModel
            {
                ItemId = 9999,
                Name = "【テスト】過去の唐揚げ",
                Status = "済",
                AddedDate = oldDate,
                UpdatedDate = oldDate
            };

            await _connection.InsertAsync(testItem);
            Console.WriteLine("✅ テストデータ挿入完了！");

            //var result = await _connection.QueryAsync<SqlResult>(
            //    "SELECT sql FROM sqlite_master WHERE type='table' AND name='CandidateCategory';");

            //var createSql = result.FirstOrDefault()?.sql;
            //System.Diagnostics.Debug.WriteLine(createSql);
        }
        public class SqlResult
        {
            public string sql { get; set; } = string.Empty;
        }

        public async Task InitializeDatabaseAsync()
        {
            Console.WriteLine("データベースの初期化を開始...");

            int retryCount = 0;
            const int maxRetries = 2;

            while (retryCount < maxRetries)
            {
                bool success = await CreateAllTablesAsync();
                if (!success)
                {
                    Console.WriteLine("テーブル作成に失敗。リカバリー処理を実行...");
                    await DropAllTablesAsync();
                    retryCount++;
                    Console.WriteLine($"リトライ {retryCount}/{maxRetries}...");
                    continue;
                }

                var latestVersion = await _connection.FindAsync<VersionDbModel>(v => v.VersionId == 1);
                if (latestVersion != null)
                {
                    Console.WriteLine("データベースはすでに初期化済み。スキップします。");
                    // ★ここでマイグレーション呼び出し！
                    var migrator = new DatabaseMigration(this); // ← ここ！
                    await migrator.MigrateIfNeededAsync();
                    //古いSoftDeleteRecordの物理削除
                    await DeleteExpiredRecordsAsync();

                    return;
                }

                Console.WriteLine("初回起動のため、初期データを投入します...");
                try
                {
                    await InsertInitialDataAsync();  // ← ここでJSONからの読み込み＋挿入
                    await InsertInitialColorMasterFromJsonAsync();  // ← ここでJSONからの読み込み＋挿入
                    Console.WriteLine("初期データの投入が完了しました！");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"データ挿入エラー: {ex.Message}");
                    retryCount++;
                    Console.WriteLine($"リカバリーを試みます... リトライ {retryCount}/{maxRetries}");
                    await DropAllTablesAsync();
                }
            }

            Console.WriteLine("データベース初期化に失敗しました。アプリを終了します。");
            throw new Exception("データベースの初期化に失敗しました。");
        }
        public async Task<string> GetCurrentDbVersionAsync()
        {
            var latestVersion = await _connection.FindAsync<VersionDbModel>(v => v.VersionId == 1);

            return latestVersion?.DbVersion ?? "0.0.0";
        }
        public async Task<bool> CreateAllTablesAsync()
        {
            try
            {
                await _connection.CreateTableAsync<VersionDbModel>();
                await _connection.CreateTableAsync<CandidateCategoryDbModel>();
                await _connection.CreateTableAsync<CandidateListItemDbModel>();
                await _connection.CreateTableAsync<ShoppingListItemDbModel>();
                await _connection.CreateTableAsync<ColorMasterDbModel>();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"テーブル作成エラー: {ex.Message}");
                return false;
            }
        }

        public async Task DropAllTablesAsync()
        {
            try
            {
                await _connection.ExecuteAsync("DROP TABLE IF EXISTS Version;");
                await _connection.ExecuteAsync("DROP TABLE IF EXISTS CandidateList;");
                await _connection.ExecuteAsync("DROP TABLE IF EXISTS CandidateListItem;");
                await _connection.ExecuteAsync("DROP TABLE IF EXISTS ShoppingListItem;");
                await _connection.ExecuteAsync("DROP TABLE IF EXISTS ColorMasterDbModel;");
                Console.WriteLine("すべてのテーブルを削除しました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"テーブル削除エラー: {ex.Message}");
            }
        }
        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, params object[] args) where T : new()
        {
            var result = await _connection.QueryAsync<T>(sql, args);
            return result.FirstOrDefault();
        }
        public async Task<T?> GetFirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            return await _connection.Table<T>().Where(predicate).FirstOrDefaultAsync();
        }
        public async Task<List<T>> TryLoadJsonFromAssets<T, TWrapper>(string fileName, Func<TWrapper, List<T>> selector)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                Console.WriteLine($"JSON 読み込み成功: {json}"); // **デバッグ用ログ**

                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };

                // **ラップクラスにデシリアライズ**
                var wrapper = JsonConvert.DeserializeObject<TWrapper>(json, settings);
                if (wrapper == null)
                {
                    Console.WriteLine($"JSON のデシリアライズに失敗: {fileName}");
                    return null;
                }

                // **指定されたリストを取得**
                var result = selector(wrapper);
                if (result == null)
                {
                    Console.WriteLine($"JSON のリスト取得に失敗: {fileName}");
                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON 読み込みエラー ({fileName}): {ex.Message}");
                return null;
            }
        }
        public async Task CreateTableAsync<T>() where T : new()
        {
            await _connection.CreateTableAsync<T>();
        }
        public async Task InsertOrReplaceAsync<T>(T item) where T : new()
        {
            await _connection.InsertOrReplaceAsync(item);
        }

        public async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
                var result = await _connection.Table<T>().Where(predicate).FirstOrDefaultAsync();
            return result != null;
        }

        public async Task<List<CandidateCategoryDbModel>> GetCandidateListDbModel()
        {
            return await _connection.Table<CandidateCategoryDbModel>().ToListAsync();
        }
        public async Task InsertInitialDataAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("InitialData.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };

                var mapping = JsonConvert.DeserializeObject<JsonMapping>(json, settings);
                if (mapping?.CandidateLists == null || mapping.CandidateLists.Count == 0)
                {
                    Console.WriteLine("初期データが空です。");
                    return;
                }

                await _connection.RunInTransactionAsync(transaction =>
                {
                    foreach (var list in mapping.CandidateLists)
                    {
                        var category = new CandidateCategoryDbModel
                        {
                            CategoryId = list.CandidateListId,
                            Title = list.Title,
                            DisplayOrder = list.DisplayOrder,
                            ColorId = list.ColorId,
                            IconName = list.IconName,
                            DeleteFlg = 0,
                            UpdatedAt = DateTimeOffset.Now,
                            IsSynced = 0
                        };
                        transaction.Insert(category);

                        foreach (var item in list.CandidateListItems)
                        {
                            var newItem = new CandidateListItemDbModel
                            {
                                CategoryId = list.CandidateListId,
                                Name = item.Name,
                                Detail = item.Detail ?? "",
                                DisplaySeq = item.DisplaySeq,
                                DeleteFlg = 0,
                                UpdatedAt = DateTimeOffset.Now,
                                IsSynced = 0
                            };
                            transaction.Insert(newItem);
                        }
                    }

                    transaction.Insert(new VersionDbModel
                    {
                        VersionId = 1,
                        DbVersion = "1.0.0",
                        UpdatedAt = DateTimeOffset.Now
                    });
                });

                Console.WriteLine("初期データ挿入完了！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InsertInitialDataAsync エラー: {ex.Message}");
                throw;
            }
        }
        //public async Task MigrateIfNeededAsync()
        //{
        //    var currentVersion = await GetCurrentDbVersionAsync();

        //    while (true)
        //    {
        //        if (currentVersion == "1.0.0")
        //        {
        //            await Migrate_1_0_0_to_1_0_1();
        //            currentVersion = "1.0.1";
        //            await SetVersionAsync(currentVersion);
        //            continue;
        //        }

        //        // これ以降のバージョンアップが追加されたらここに追記

        //        break; // 最新状態ならループ終了
        //    }
        //}
        public async Task SetVersionAsync(string version)
        {
            //var conn = await GetConnectionAsync();
            var newVersion = new VersionDbModel
            {
                VersionId = 1,
                DbVersion = version,
                UpdatedAt = DateTimeOffset.Now
            };
            await _connection.InsertOrReplaceAsync(newVersion);
        }

        


        public async Task InsertInitialColorMasterFromJsonAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("ColorMaster.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // ← JSONのプロパティ名の大文字小文字を無視する設定
            };
            var colorList = System.Text.Json.JsonSerializer.Deserialize<List<ColorMasterDbModel>>(json, options);


            if (colorList?.Count > 0)
            {
                await _connection.InsertAllAsync(colorList);
            }
        }

        public async Task<List<CandidateCategoryDbModel>> GetCandidateCategoryDbModels()
        {
            return await _connection.Table<CandidateCategoryDbModel>().ToListAsync();
        }
        public async Task<List<CandidateListItemDbModel>> GetCandidateListItemDbModelsByCategoryId(int categoryId)
        {
            return await _connection.Table<CandidateListItemDbModel>()
                .Where(x => x.CategoryId == categoryId)
                .ToListAsync();
        }
        public async Task<List<string>> GetActiveShoppingItemNamesByCategoryIdAsync(int categoryId)
        {
            var candidateItems = await _connection.Table<CandidateListItemDbModel>()
                .Where(i => i.CategoryId == categoryId && i.DeleteFlg != 1)
                .ToListAsync();

            if (candidateItems.Count == 0)
                return new List<string>();

            var itemIds = candidateItems.Select(i => i.ItemId).ToList();

            var shoppingItems = await _connection.Table<ShoppingListItemDbModel>()
                .Where(s => itemIds.Contains(s.ItemId) && (s.Status == null || s.Status != "済"))
                .ToListAsync();

            var activeItemNames = candidateItems
                .Where(ci => shoppingItems.Any(si => si.ItemId == ci.ItemId))
                .Select(ci => ci.Name)
                .ToList();

            return activeItemNames;
        }

        // DatabaseService.cs に追加
        public async Task<int> InsertAsync<T>(T item) where T : new()
        {
            await _connection.InsertAsync(item);
            var pk = typeof(T).GetProperty("ItemId")?.GetValue(item);
            return pk is int id ? id : 0;
            //return _connection.InsertAsync(item);
        }

        public Task<int> UpdateAsync<T>(T item) where T : new()
        {
            try
            {
                return _connection.UpdateAsync(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine("★★ DB更新エラー: " + ex.Message);
                return Task.FromResult(-1); // エラー時に -1 を返す
            }
        }
        public async Task<T?> GetAsync<T>(int id) where T : new()
        {
            string tableName = typeof(T).Name.Replace("DbModel", ""); // 例: CandidateCategoryDbModel → CandidateCategory
            string query = $"SELECT * FROM {tableName} WHERE {tableName}Id = ?";
            var result = await QueryAsync<T>(query, id);
            return result.FirstOrDefault();
        }
        public async Task<T?> GetCandidateCategoryAsync<T>(int id) where T : new()
        {
            string tableName = typeof(T).Name.Replace("DbModel", ""); // 例: CandidateCategoryDbModel → CandidateCategory
            string query = $"SELECT * FROM {tableName} WHERE CategoryId = ?";
            var result = await QueryAsync<T>(query, id);
            return result.FirstOrDefault();
        }
        public async Task<Dictionary<int, ColorSet>> GetColorSetMapAsync()
        {
            try
            {
                var list = await _connection.Table<ColorMasterDbModel>().ToListAsync();

                var dict = list.ToDictionary(
                    c => c.ColorId,
                    c => new ColorSet
                    {
                        Unselected = Color.FromArgb(c.UnSelectedHexCode),
                        Selected = Color.FromArgb(c.SelectedHexCode),
                        PreSelected = Color.FromArgb(c.PreSelectedHexCode)
                    });

                return dict;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }


        public async Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new()
        {
            try
            {
                var rts = await _connection.QueryAsync<T>(query, args);
                Console.WriteLine($"QueryAsync 件数: {rts.Count}");

                return rts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"QueryAsync でエラー: {ex.Message}");
                return new List<T>();
            }
        }
        public async Task<List<T>> GetAllAsync<T>() where T : new()
        {
            //var conn = await GetConnectionAsync();
            return await _connection.Table<T>().ToListAsync();
        }
        //public TableQuery<T> GetTable<T>() where T : new()
        //{
        //    return _connection.Table<T>();
        //}
        public AsyncTableQuery<T> GetTable<T>() where T : new()
        {
            return _connection.Table<T>();
        }
        public Task<List<CandidateListItemDbModel>> GetCandidateItemsByCategoryIdAsync(int categoryId)
        {
            return _connection.Table<CandidateListItemDbModel>()
                      .Where(i => i.CategoryId == categoryId && i.DeleteFlg != 1)
                      .ToListAsync();
        }

        public Task<List<ShoppingListItemDbModel>> GetShoppingListItemsAsync()
        {
            return _connection.Table<ShoppingListItemDbModel>()
                      .Where(i => i.Status != null)
                      .ToListAsync();
        }

        public async Task DeleteCandidateCategoryAsync(int categoryId)
        {
            var category = await _connection.Table<CandidateCategoryDbModel>()
                                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category != null)
            {
                category.DeleteFlg = 1;
                category.UpdatedAt = DateTimeOffset.Now;
                await _connection.UpdateAsync(category);
            }
        }
        public async Task SetSettingAsync(string key, string value)
        {
            //var conn = await GetConnectionAsync();
            var setting = new AppSettingDbModel
            {
                Key = key,
                Value = value,
                UpdatedDate = DateTimeOffset.Now
            };
            await _connection.InsertOrReplaceAsync(setting);
        }
        public async Task<string?> GetSettingAsync(string key)
        {
            //var conn = await GetConnectionAsync();
            var setting = await _connection.Table<AppSettingDbModel>()
                                    .FirstOrDefaultAsync(s => s.Key == key);
            return setting?.Value;
        }
        public async Task DeleteExpiredRecordsAsync()
        {
            try
            {
                var days = _settingsService.GetRetentionDays();
                var cutoffDate = DateTimeOffset.Now.AddDays(-days);

                // Before
                var catBefore = await _connection.Table<CandidateCategoryDbModel>()
                    .Where(x => x.DeleteFlg == 1 && x.UpdatedAt <= cutoffDate)
                    .CountAsync();

                var itemBefore = await _connection.Table<CandidateListItemDbModel>()
                    .Where(x => x.DeleteFlg == 1 && x.UpdatedAt <= cutoffDate)
                    .CountAsync();

                var shopBefore = await _connection.Table<ShoppingListItemDbModel>()
                    .Where(x => x.Status == "済" && x.UpdatedDate <= cutoffDate)
                    .CountAsync();

                Console.WriteLine($"🔍 削除対象件数: Category={catBefore}, Item={itemBefore}, Shopping={shopBefore}");

                // 実際の削除
                await _connection.ExecuteAsync(@"DELETE FROM CandidateCategory WHERE DeleteFlg = 1 AND UpdatedAt <= ?", cutoffDate);
                await _connection.ExecuteAsync(@"DELETE FROM CandidateListItem WHERE DeleteFlg = 1 AND UpdatedAt <= ?", cutoffDate);
                await _connection.ExecuteAsync(@"DELETE FROM ShoppingListItem WHERE Status = '済' AND UpdatedDate <= ?", cutoffDate);

                // After（確認）
                var catAfter = await _connection.Table<CandidateCategoryDbModel>().CountAsync();
                var itemAfter = await _connection.Table<CandidateListItemDbModel>().CountAsync();
                var shopAfter = await _connection.Table<ShoppingListItemDbModel>().CountAsync();

                Console.WriteLine($"✅ 削除後件数: Category={catAfter}, Item={itemAfter}, Shopping={shopAfter}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ レコード削除エラー: {ex.Message}");
            }
        }
        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            return _connection;
        }

        public async Task<int> ExecuteAsync(string sql, params object[] args)
        {
            try { return await _connection.ExecuteAsync(sql, args); }
            catch (Exception ex)
            {
                Console.WriteLine($"ExecuteAsync でエラー: {ex.Message}");
                return -1;
            }
        }

    }
}

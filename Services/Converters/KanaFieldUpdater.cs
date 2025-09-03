using ShoppingList002.Models.DbModels;
using ShoppingList002.Services.Converters;
using SQLite;

namespace ShoppingList002.Services.Converter;

public static class KanaFieldUpdater
{
    // 1件用：追加/更新の直前に呼ぶ
    public static void EnsureKanaFields(CandidateListItemDbModel item)
    {
        // ✅ ベース読みは “必ず” トークナイザ（Java）由来
        var baseKana = KanaHelper.ToHiragana(item.Name); // 例: 「豚こま」→「ぶたこま」

        // 既存の揺らぎ展開（ぶた/とん・大葉/しそ 等）
        var keys = KanaConverter.GenerateKanaKeysSimple(item.Name);

        // ✅ SearchKana は順序維持で baseKana を先頭に必ず含める
        var ordered = new List<string>();
        if (!string.IsNullOrEmpty(baseKana)) ordered.Add(baseKana);
        foreach (var k in keys)
        {
            if (!string.IsNullOrEmpty(k) && !ordered.Contains(k))
                ordered.Add(k);
        }

        // ✅ Kana は常にベース読み（フォールバックは ordered 先頭）
        item.Kana = !string.IsNullOrEmpty(baseKana) ? baseKana : ordered.FirstOrDefault();
        item.SearchKana = string.Join(" ", ordered);
    }

    // 全件用：起動時/マイグレーション等で呼ぶ
    public static async Task<int> EnsureKanaFieldsForAllAsync(SQLiteAsyncConnection db)
    {
        var list = await db.Table<CandidateListItemDbModel>().ToListAsync();
        var cnt = 0;

        foreach (var it in list)
        {
            var beforeK = it.Kana ?? "";
            var beforeS = it.SearchKana ?? "";

            EnsureKanaFields(it);

            if (it.Kana != beforeK || it.SearchKana != beforeS)
            {
                await db.UpdateAsync(it);
                cnt++;
            }
        }
        return cnt;
    }
    // 一括用：起動時やマイグレーションで使う
    public static async Task<int> RebuildAllAsync(IDatabaseService db)
    {
        var items = await db.GetAllAsync<CandidateListItemDbModel>();
        int cnt = 0;
        foreach (var it in items.Where(x => x.DeleteFlg != 1))
        {
            var beforeK = it.Kana ?? "";
            var beforeS = it.SearchKana ?? "";

            EnsureKanaFields(it);

            if (it.Kana != beforeK || it.SearchKana != beforeS)
            {
                await db.UpdateAsync(it);
                cnt++;
            }
        }
        return cnt;
    }
}

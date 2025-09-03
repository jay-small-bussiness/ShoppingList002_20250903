using ShoppingList002.Models.UiModels;
using ShoppingList002.Models.DbModels;
using ShoppingList002.Models;
using ShoppingList002.Services.Converters;
using ShoppingList002.Exceptions;
using ShoppingList002.Services.Converter;
//using Android.App.Job;

namespace ShoppingList002.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly IDatabaseService _databaseService;

        public CandidateService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<CandidateCategoryDbModel>> GetCandidateCategoriesAsync()
        {
           
            var query = "SELECT CategoryId, Title, ColorId, IconName, DisplayOrder FROM CandidateCategory WHERE DeleteFlg = 0 ORDER BY DisplayOrder";
            var dbModels = await _databaseService.QueryAsync<CandidateCategoryDbModel>(query,"");
            Console.WriteLine($"カテゴリ取得件数: {dbModels.Count}");
            return dbModels;
        }
        public async Task<List<CandidateListItemDbModel>> GetCandidateItemsAsync(int categoryId)
        {
            var sql = "SELECT ItemId, CategoryId, Name, Detail, DisplaySeq FROM CandidateListItem WHERE CategoryId = ? AND DeleteFlg = 0 ORDER BY DisplaySeq";
            return await _databaseService.QueryAsync<CandidateListItemDbModel>(sql, categoryId);

        }
        private async Task<int> GetNextDisplaySeqAsync(int categoryId)
        {
            string sql = "SELECT * FROM CandidateListItem WHERE CategoryId = ? AND DeleteFlg != 1";
            var existing = await _databaseService.QueryAsync<CandidateListItemDbModel>(sql, categoryId);

            //var existing = await _databaseService.GetListAsync<CandidateListItemDbModel>(
            //    x => x.CategoryId == categoryId && x.DeleteFlg != 1);

            return existing.Any() ? existing.Max(x => x.DisplaySeq) + 1 : 1;
        }
        // ===================== 検索本体（差し替え案）=====================
        // ポイント：まず SearchKana（厳密）→ ダメなら Kana（素直）で拾う二段構え
        // - 例：「たっぷり」 → SearchKana同士ではNoHit → Kanaの部分一致で「野菜たっぷりドレッシング」を拾う
        // - 例：「もも」/「ろーす」 → Kanaの後方一致で「鶏もも」/「豚ロース」を拾う（必要に応じてEndsWith有効）
        // ================================================================

        public async Task<List<SearchResultItemModel>> SearchByNameAsync(string input)
        {

            // 0) 入力バリデーション
            if (string.IsNullOrWhiteSpace(input))
                return new List<SearchResultItemModel>(); // ← 空文字なら即終了

            // 1) 入力の正規化キー作成（ユーザーの発話・入力を2系統のキーに）
            var keyStrict = KanaHelper.ToSearchKana(input);   // ← 検索用：促音/長音統一、濁点処理など“機械的キー”
            var keyKana = KanaHelper.ToKana(input);         // ← 表示読み寄り：ひらがな化＆空白除去（濁点は保持）

            // 2) 検索対象の取得（削除フラグ除外）
            var allItems = (await _databaseService.GetAllAsync<CandidateListItemDbModel>())
                           .Where(x => x.DeleteFlg != 1)
                           .ToList();                         // ← 一旦メモリへ（件数少ならOK）

            var allCats = await _databaseService.GetAllAsync<CandidateCategoryDbModel>(); // ← カテゴリ名付与用

            // 3) アイテム側の比較キー取得関数（無ければ生成でフォールバック）
            //string ItemSearchKey(CandidateListItemDbModel x)
            //    => !string.IsNullOrEmpty(x.SearchKana) ? x.SearchKana! : KanaHelper.ToSearchKana(x.Name); // ← SearchKana優先
            string ItemSearchKey(CandidateListItemDbModel x)
                => KanaHelper.ToSearchKana(x.Name);

            string ItemKanaKey(CandidateListItemDbModel x)
                => !string.IsNullOrEmpty(x.Kana) ? x.Kana! : KanaHelper.ToKana(x.Name);                    // ← Kana優先

            // 4) マッチ関数（OrdinalでOK。必要ならIgnoreCaseへ）
            static bool Starts(string t, string q) => t.StartsWith(q, StringComparison.Ordinal);   // ← 前方一致
            static bool Ends(string t, string q) => t.EndsWith(q, StringComparison.Ordinal);   // ← 後方一致
            static bool Contains2(string t, string q) => t.Contains(q, StringComparison.Ordinal);   // ← 部分一致

            // 5) スコアリング結果（アイテムと“なぜ当たったか”のメモ付き）
            var scored = new List<(CandidateListItemDbModel item, int score, string reason)>();
            var cab = allItems.FirstOrDefault(x => x.Name == "キャベツ");
            //if (cab != null)
            //{
            //    Console.WriteLine($"キャベツ: SearchKana={cab.SearchKana}, Kana={cab.Kana}, ToSearchKana(Name)={KanaHelper.ToSearchKana(cab.Name)}");
            //}
            // --- 第一段：SearchKana 同士の照合（“機械的キー”で前方→部分の順）
            foreach (var it in allItems)
            {
                var s = ItemSearchKey(it);                        // ← アイテム側の SearchKana
                if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(keyStrict)) continue;

                if (Starts(s, keyStrict))                         // ← 例：「ろす」等の長音省略にもそこそこ強い
                {
                    scored.Add((it, 100, "SearchKana:前方一致")); // ← 最優先
                    continue;                                     // ← 同一アイテムで下の判定へは行かない
                }
                if (Contains2(s, keyStrict))
                {
                    scored.Add((it, 90, "SearchKana:部分一致")); // ← 次点
                    continue;
                }
            }

            // --- 第二段：Kana 同士の照合（“人の読み”で自然に拾う）
            // ここで「たっぷり」→「やさいたっぷりどれっしんぐ」にヒットする（部分一致）
            foreach (var it in allItems)
            {
                // すでに第一段で拾えてたらスキップしてもいいが、
                // 同一アイテムのより高いスコアが既に入ってるだけなので重複除去フェーズで解消される
                var k = ItemKanaKey(it);                          // ← アイテム側の Kana（ひらがな、濁点保持）
                if (string.IsNullOrEmpty(k) || string.IsNullOrEmpty(keyKana)) continue;

                // （任意）短語の誤爆が気になるなら MinLen を2〜3にして Contains/Endsを制限
                const int MinLen = 2;

                if (keyKana.Length >= MinLen && Starts(k, keyKana))
                    scored.Add((it, 80, "Kana:前方一致"));        // ← 例：「ごま」→「ごまドレッシング」
                if (keyKana.Length >= MinLen && Ends(k, keyKana))
                    scored.Add((it, 78, "Kana:後方一致"));        // ← 例：「もも」→「鶏もも」、「ろーす」→「豚ロース」
                if (keyKana.Length >= MinLen && Contains2(k, keyKana))
                    scored.Add((it, 75, "Kana:部分一致"));        // ← 例：「たっぷり」→「野菜たっぷりドレッシング」
            }

            // 6) アイテムごとに最高スコアだけ残して、降順ソート
            var results = scored
                .GroupBy(x => x.item.ItemId)                      // ← 同じアイテムはまとめる
                .Select(g =>
                {
                    var best = g.OrderByDescending(xx => xx.score).First();
                    return (best.item, best.score, best.reason);
                })
                .OrderByDescending(x => x.score)                  // ← 高スコア順に並べる
                .Select(x => new SearchResultItemModel
                {
                    ItemId = x.item.ItemId,
                    //CategoryId = x.item.CategoryId,
                    ItemName = x.item.Name,
                    CategoryName = allCats.FirstOrDefault(c => c.CategoryId == x.item.CategoryId)?.Title ?? "",
                    Score = x.score,
                    // （任意）UIで“なぜ当たったか”を薄文字表示したい場合は下みたいなフィールドをモデルに追加
                    // HitReason   = x.reason
                })
                .ToList();

            return results;                                       // ← 完了！
        }


        //// CandidateService.cs （SearchByNameAsyncを差し替え）
        //public async Task<List<SearchResultItemModel>> SearchByNameAsync(string input, bool useLoosen = false)
        //{
        //    if (string.IsNullOrWhiteSpace(input))
        //        return new List<SearchResultItemModel>();

        //    // 正規化キー作成
        //    var keyStrict = KanaHelper.ToSearchKana(input);
        //    var keyLoosen = KanaHelper.ToSearchKanaLoosen(input);
        //    var keyNameRaw = input; // Nameの生テキスト用

        //    var allItems = await _databaseService.GetAllAsync<CandidateListItemDbModel>();
        //    var allCats = await _databaseService.GetAllAsync<CandidateCategoryDbModel>();

        //    // 検索対象の各アイテムについて、事前に正規化済みフィールドを準備
        //    // （既にDBにSearchKanaが入ってる場合はそれ優先、無い場合は都度生成）
        //    string ItemStrictKey(CandidateListItemDbModel x)
        //        => string.IsNullOrEmpty(x.SearchKana) ? KanaHelper.ToSearchKana(x.Name) : x.SearchKana!;
        //    string ItemLoosenKey(CandidateListItemDbModel x)
        //        => KanaHelper.ToSearchKanaLoosen(x.Name);

        //    // スコア付き結果を集める
        //    var scored = new List<(CandidateListItemDbModel item, int score)>();

        //    bool StartsWith(string target, string key) => target.StartsWith(key, StringComparison.Ordinal);
        //    bool Contains(string target, string key) => target.Contains(key, StringComparison.Ordinal);

        //    foreach (var x in allItems.Where(i => i.DeleteFlg != 1))
        //    {
        //        var strict = ItemStrictKey(x);

        //        // Strict: StartsWith → Contains
        //        if (!string.IsNullOrEmpty(strict))
        //        {
        //            if (StartsWith(strict, keyStrict)) { scored.Add((x, 100)); continue; }
        //            if (Contains(strict, keyStrict)) { scored.Add((x, 90)); continue; }
        //        }

        //        // Nameの生テキストでも一応拾う（スコア低め）
        //        if (!string.IsNullOrEmpty(x.Name))
        //        {
        //            // 大文字小文字は日本語では関係薄いが念のため OrdinalIgnoreCase にしない（パフォ&誤爆防止）
        //            if (x.Name.Contains(keyNameRaw, StringComparison.Ordinal))
        //            {
        //                scored.Add((x, 50));
        //            }
        //        }
        //    }

        //    // Loosenは「0件 or 2件以下」の時だけ、またはUIからの拡張指示（useLoosen=true）
        //    if (useLoosen || scored.Count <= 2)
        //    {
        //        foreach (var x in allItems.Where(i => i.DeleteFlg != 1))
        //        {
        //            var loosen = ItemLoosenKey(x);
        //            if (string.IsNullOrEmpty(loosen)) continue;

        //            // 入力長に応じて許容範囲を変える（2以下はStartsWithのみ）
        //            if (keyLoosen.Length <= 2)
        //            {
        //                if (StartsWith(loosen, keyLoosen))
        //                    scored.Add((x, 70));
        //            }
        //            else
        //            {
        //                if (StartsWith(loosen, keyLoosen)) { scored.Add((x, 70)); continue; }
        //                if (Contains(loosen, keyLoosen)) { scored.Add((x, 60)); }
        //            }
        //        }
        //    }

        //    // 重複排除（ItemIdでユニーク化）、スコア降順
        //    var unique = scored
        //        .GroupBy(t => t.item.ItemId)
        //        .Select(g => g.OrderByDescending(t => t.score).First())
        //        .OrderByDescending(t => t.score)
        //        .ToList();

        //    // モデル変換
        //    var colorMap = await _databaseService.GetColorSetMapAsync();
        //    var catMap = allCats.ToDictionary(c => c.CategoryId, c => c);

        //    var results = new List<SearchResultItemModel>();
        //    foreach (var (item, score) in unique)
        //    {
        //        var cat = catMap.TryGetValue(item.CategoryId, out var c) ? c : null;
        //        var color = colorMap.TryGetValue(cat?.ColorId ?? 0, out var cs) ? cs : default;

        //        results.Add(new SearchResultItemModel
        //        {
        //            ItemId = item.ItemId,
        //            //CategoryId = item.CategoryId,
        //            ItemName = item.Name,
        //            CategoryName = cat?.Title ?? "",
        //            Score = score,
        //            BackgroundColor = color.Unselected
        //        });
        //    }

        //    return results;
        //}


        public async Task CopyItemToCategoryAsync(CandidateListItemUiModel sourceItem, int targetCategoryId)
        {
            // 複製用のUIモデルを新規作成
            var newItem = new CandidateListItemUiModel
            {
                CategoryId = targetCategoryId,
                Name = sourceItem.Name,
                Detail = sourceItem.Detail,
                DisplaySeq = await GetNextDisplaySeqAsync(targetCategoryId), // ←あとで定義
                ColorId = sourceItem.ColorId,
                IsInShoppingList = false,
                CategoryTitle = "",
                BackgroundColor = sourceItem.BackgroundColor
            };

            var dbModel = newItem.ToDbModel(); // 拡張メソッドでDBモデルへ変換
            await _databaseService.InsertAsync(dbModel);
        }
        public async Task<CandidateCategoryDbModel> FindCategoryByNameAsync(string name)
        {
            var all = await GetCandidateCategoriesAsync(); // 全カテゴリ取得
            return all.FirstOrDefault(c => c.Title == name); // 完全一致
        }

        public async Task MoveItemToCategoryAsync(CandidateListItemUiModel sourceItem, int targetCategoryId)
        {
            // ① コピー先に新規追加
            var newItem = new CandidateListItemUiModel
            {
                CategoryId = targetCategoryId,
                Name = sourceItem.Name,
                Detail = sourceItem.Detail,
                DisplaySeq = await GetNextDisplaySeqAsync(targetCategoryId),
                ColorId = sourceItem.ColorId,
                IsInShoppingList = false,
                BackgroundColor = sourceItem.BackgroundColor
            };

            var dbNewItem = newItem.ToDbModel();
            await _databaseService.InsertAsync(dbNewItem);

            // ② 元アイテムをソフトデリート
            var dbOldItem = sourceItem.ToDbModel();
            dbOldItem.DeleteFlg = 1;
            dbOldItem.UpdatedAt = DateTimeOffset.Now;
            await _databaseService.UpdateAsync(dbOldItem);
        }

        public async Task<List<CandidateListItemUiModel>> GetCandidateItemsByCategoryAsync(int categoryId)
        {
            var sql = @"
                        SELECT c.ItemId, c.CategoryId, c.Name, c.Detail, c.DisplaySeq,
                               s.Id IS NOT NULL AS IsInShoppingList
                        FROM CandidateListItem c
                        LEFT JOIN ShoppingListItem s
                          ON c.ItemId = s.ItemId AND s.Status IS NULL
                        WHERE c.CategoryId = ? AND c.DeleteFlg = 0
                        ORDER BY c.DisplaySeq";

            var dbResults = await _databaseService.QueryAsync<CandidateListItemWithCheckDbModel>(sql, categoryId);

            // 変換してUIモデルにする
            return dbResults.Select(item => new CandidateListItemUiModel
            {
                ItemId = item.ItemId,
                Name = item.Name,
                Detail = item.Detail,
                DisplaySeq = item.DisplaySeq,
                CategoryId = item.CategoryId,
                IsInShoppingList = item.IsInShoppingList
            }).ToList();
        }
        public async Task<Dictionary<int, ColorSet>> GetColorMapAsync()
        {
            return await _databaseService.GetColorSetMapAsync();
        }

        public async Task DeleteCandidateItemAsync(int itemId)
        {
            var dbItem = await _databaseService.GetFirstOrDefaultAsync<CandidateListItemDbModel>(x => x.ItemId == itemId);
            if (dbItem != null)
            {
                dbItem.DeleteFlg = 1;
                dbItem.UpdatedAt = DateTimeOffset.Now;
                await _databaseService.UpdateAsync(dbItem);
            }
        }

        public async Task<CandidateCategoryDbModel?> GetCategoryByIdAsync(int categoryId)
        {
            return await _databaseService.GetCandidateCategoryAsync<CandidateCategoryDbModel>(categoryId);
        }

        public async Task<bool> CanDeleteCategoryAsync(int categoryId)
        {
            var items = await _databaseService.GetCandidateItemsByCategoryIdAsync(categoryId);

            if (items.Count == 0)
                return true;

            var activeNames = await _databaseService.GetActiveShoppingItemNamesByCategoryIdAsync(categoryId);

            if (activeNames.Count > 0)
            {
                throw new CategoryNotEmptyException(
                    items.Select(i => i.Name).ToList(),
                    activeNames
                );
            }

            // アイテムは残ってるけど、ShoppingListに乗ってるやつはない →削除NG
            throw new CategoryNotEmptyException(
                items.Select(i => i.Name).ToList(),
                new List<string>()
            );
        }
        public async Task UpdateCategoryAsync(CandidateCategoryDbModel model)
        {
            await _databaseService.UpdateAsync(model);
        }

        public async Task<int> InsertCategoryAsync(CandidateCategoryDbModel model)
        {
            return await _databaseService.InsertAsync(model);
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            await _databaseService.DeleteCandidateCategoryAsync(categoryId);
        }
        public async Task<List<SearchResultItemModel>> SearchItemsAsync(string keyword)
        {
            var items = await _databaseService.GetTable<CandidateListItemDbModel>()
                .Where(x => x.Name.Contains(keyword)) // SQLiteでも動く部分一致
                .ToListAsync();

            var categoryIds = items.Select(x => x.CategoryId).Distinct().ToList();

            var categories = await _databaseService.GetTable<CandidateCategoryDbModel>()
                .Where(c => categoryIds.Contains(c.CategoryId))
                .ToListAsync();

            var categoryMap = categories.ToDictionary(c => c.CategoryId, c => c.Title);

            var result = items.Select(item => new SearchResultItemModel
            {
                ItemId = item.ItemId,
                ItemName = item.Name,
                CategoryName = categoryMap.TryGetValue(item.CategoryId, out var title) ? title : "未分類"
                }).ToList();

            return result;
        }
        // CandidateService など、Insert/Updateの共通フック
        //この部分は
        //   Services/
        //├─ Converter/
        //│   ├─ KanaConverter.cs         // 文字→かなキー群の生成（正規化 & ゆらぎ展開）
        //│   └─ KanaFieldUpdater.cs
        //private void EnsureKanaFields(CandidateListItemDbModel item)
        //{
        //    // 代表キー生成（今回の合意：発話もマスターも同じ正規化）
        //    // まず素直なひらがな正規化
        //    string baseKey = KanaHelper.ToHiraganaLoose(item.Name); // = ひらがな化・空白/小書き統一・記号除去（濁点は残す）

        //    // 読みのゆらぎ展開（豚→ぶた/とん、鶏→にわとり/とり 等）
        //    var keys = KanaHelper.GenerateKanaKeysSimple(item.Name); // ["ぶたとろ","とんとろ"] など

        //    // 表示寄り：ひとまず baseKey をそのまま
        //    item.Kana = baseKey;

        //    // キャッシュ用途の代表キー（最初の1本だけ）
        //    item.SearchKana = keys.Count > 0 ? keys[0] : baseKey;
        //}

        //// 追加
        //public async Task<int> AddItemAsync(CandidateListItemDbModel item)
        //{
        //    return await _databaseService.InsertAsync(item);
        //}

        //// 更新
        //public async Task<int> UpdateItemAsync(CandidateListItemDbModel item)
        //{
        //    EnsureKanaFields(item);
        //    return await _databaseService.UpdateAsync(item);
        //}
        public async Task<int> AddCandidateItemAsync(CandidateListItemDbModel item)
        {
            KanaFieldUpdater.EnsureKanaFields(item);
            return await _databaseService.InsertAsync(item);
        }
        public async Task UpdateCandidateItemAsync(CandidateListItemUiModel uiModel)
        {
            var dbModel = uiModel.ToDbModel();
            KanaFieldUpdater.EnsureKanaFields(dbModel);
            await _databaseService.UpdateAsync(dbModel);
        }

    }
}
    
using ShoppingList002.Models.UiModels;
using ShoppingList002.Models.DbModels;
//using Android.App.Job;

namespace ShoppingList002.Services
{
    public class ShoppingListService : IShoppingListService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ActivityLogService _activityLogService;


        public ShoppingListService(IDatabaseService databaseService, ActivityLogService activityLogService)
        {
            _databaseService = databaseService;
            _activityLogService = activityLogService;
        }

        public async Task AddToShoppingListAsync(ShoppingListItemDbModel model)
        {
            // 同じItemIdですでに登録中のやつがあるかチェック
            var existing = await _databaseService.GetFirstOrDefaultAsync<ShoppingListItemDbModel>(
                x => x.ItemId == model.ItemId && x.Status == null);
                
            if (existing != null)
            {
                // すでに登録済なので何もしない（orトースト表示など）
                return;
            }

            var now = DateTime.Now;

            var newItem = new ShoppingListItemDbModel
            {
                ItemId = model.ItemId,
                Name = model.Name,
                Detail = model.Detail,
                AddedDate = now,
                UpdatedDate = now,
                Status = null // 登録中
            };

            await _databaseService.InsertAsync(newItem);
        }
        public async Task CancelShoppingListItemAsync(int itemId)
        {
            var existing = await _databaseService.GetFirstOrDefaultAsync<ShoppingListItemDbModel>(
                x => x.ItemId == itemId && x.Status == null);

            if (existing != null)
            {
                existing.Status = "C";
                existing.UpdatedDate = DateTime.Now;
                await _databaseService.UpdateAsync(existing);
            }
        }
        public async Task<bool> ExistsAsync(int itemId)
        {
            string sql = "SELECT * FROM ShoppingListItem WHERE ItemId = ? AND Status IS NULL";
            var result = await _databaseService.QueryAsync<ShoppingListItemDbModel>(sql, itemId);
            return result.Any();
        }
        public async Task AddItemsAsync(IEnumerable<CandidateListItemUiModel> items)
        {
            foreach (var item in items)
            {
                // 例：すでに登録済みかチェック（重複防止）
                var exists = await _databaseService.ExistsAsync<ShoppingListItemDbModel>(
                    x => x.ItemId == item.ItemId && x.Status == null);

                if (!exists)
                {
                    var newItem = new ShoppingListItemDbModel
                    {
                        ItemId = item.ItemId,
                        Name = item.Name,
                        Detail = item.Detail,
                        AddedDate = DateTime.Now
                    };

                    await _databaseService.InsertAsync(newItem);
                }
            }
        }

        public async Task<List<ShoppingListUiModel>> GetDisplayItemsAsync()
        {
            var shoppingItems = await _databaseService.QueryAsync<ShoppingListItemDbModel>(
                "SELECT * FROM ShoppingListItem WHERE Status IS NULL OR Status = ''");
                //"SELECT * FROM ShoppingListItem WHERE Status IS NULL AND IsDeleted = 0");

            var allCandidateItems = await _databaseService.GetAllAsync<CandidateListItemDbModel>();
            var allCategories = await _databaseService.GetAllAsync<CandidateCategoryDbModel>(); // ←カテゴリー
            var colorMap = await _databaseService.GetColorSetMapAsync();

            var result = new List<ShoppingListUiModel>();

            foreach (var sItem in shoppingItems)
            {
                var cItem = allCandidateItems.FirstOrDefault(x => x.ItemId == sItem.ItemId);
                if (cItem == null) continue;

                var category = allCategories.FirstOrDefault(x => x.CategoryId == cItem.CategoryId);
                if (category == null) continue;

                if (!colorMap.TryGetValue(category.ColorId, out var colorSet))
                    continue;

                result.Add(new ShoppingListUiModel
                {
                    ItemId = sItem.ItemId,
                    Name = sItem.Name,
                    Detail = sItem.Detail,
                    CategoryId = category.CategoryId,
                    CategoryTitle = category.Title,
                    CategoryDisplayOrder = category.DisplayOrder,
                    ItemDisplaySeq = cItem.DisplaySeq,
                    AddedDate = sItem.AddedDate,
                    BackgroundColor = colorSet.Unselected // ← 選択中表示にはUnselected色
                });
            }

            return result
                .OrderBy(x => x.CategoryDisplayOrder)
                .ThenBy(x => x.ItemDisplaySeq)
                .ToList();
        }
        public async Task MarkAsPurchasedAsync(int itemId)
        {
            var item = await _databaseService.GetFirstOrDefaultAsync<ShoppingListItemDbModel>(
                x => x.ItemId == itemId && x.Status == null);

            if (item != null)
            {
                item.Status = "済";
                item.UpdatedDate = DateTime.Now;
                await _databaseService.UpdateAsync(item);
            }
            await LogPurchasedAddAsync(item.Name, itemId, "");

        }
        public async Task<string?> UndoLastPurchasedItemAsync()
        {
            var limit = DateTime.Now.AddHours(-24);

            // 最新の「済」を1件だけ取得（UpdatedDateの降順）
            var latest = await _databaseService.QueryFirstOrDefaultAsync<ShoppingListItemDbModel>(
                "SELECT * FROM ShoppingListItem WHERE Status = '済' AND UpdatedDate >= ? ORDER BY UpdatedDate DESC LIMIT 1", limit);
            if (latest != null)
            {
                latest.Status = null; // 戻す
                latest.UpdatedDate = DateTime.Now;
                await _databaseService.UpdateAsync(latest);
                return latest.Name; // ← UIに返してToast出せる
            }
            return null;
        }
        private async Task LogPurchasedAddAsync(string itemName, int itemId, string categoryName)
        {
            await _activityLogService.InsertLogAsync(
                actionType: "購入",
                itemName: itemName,
                categoryName: categoryName,
                itemId: itemId
            );
        }
        public async Task<List<int>> GetActiveItemIdsAsync()
        {
            var items = await _databaseService.QueryAsync<ShoppingListItemDbModel>(
                "SELECT ItemId FROM ShoppingListItem WHERE Status IS NULL OR Status = ''");

            return items.Select(x => x.ItemId).Distinct().ToList();
        }
        public async Task AddItemAsync(int itemId)
        {
            // 候補アイテム取得
            var candidate = await _databaseService.GetFirstOrDefaultAsync<CandidateListItemDbModel>(x => x.ItemId == itemId);

            if (candidate == null)
                return; // ない場合はスルー

            var newItem = new ShoppingListItemDbModel
            {
                Id = 0,  // AutoIncrement
                ItemId = itemId, 
                Name = candidate.Name,
                //CategoryId = candidate.CandidateListId,
                AddedDate = DateTimeOffset.Now,
                UpdatedDate = DateTimeOffset.Now,
                Status = null
            };

            await _databaseService.InsertAsync(newItem);
        }
    }
}

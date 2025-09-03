using ShoppingList002.Models.DbModels;

namespace ShoppingList002.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ISettingsService _settingsService;

        public ActivityLogService(IDatabaseService databaseService, ISettingsService settingsService)
        {
            _databaseService = databaseService;
            _settingsService = settingsService;
        }

        public async Task InsertLogAsync(string actionType, string itemName, string categoryName, int? itemId = null)
        {
            var log = new ActivityLogDbModel
            {
                ActionType = actionType,
                ItemId = itemId,
                ItemName = itemName,
                CategoryName = categoryName,
                //Actor = _settingsService.GetCurrentActorName(), // ← 未実装なら "この端末" とかで仮置き
                Actor = "この端末",
                Timestamp = DateTimeOffset.Now,
                Description = $"{itemName}（{categoryName}）を{actionType}"
            };

            await _databaseService.InsertAsync(log);
        }
        public async Task LogAsync(string actionType, int? itemId, string itemName, string categoryName, string description)
        {
            var log = new ActivityLogDbModel
            {
                ActionType = actionType,
                ItemId = itemId,
                ItemName = itemName,
                CategoryName = categoryName,
                Description = description,
                Actor = "じゅんちゃん", // 仮設定。将来ユーザー名取得可に
                Timestamp = DateTimeOffset.Now
            };

            await _databaseService.InsertAsync(log);
        }
        public async Task<List<ActivityLogDbModel>> GetLogsAsync()
        {
            return await _databaseService.GetTable<ActivityLogDbModel>()
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
    }

}

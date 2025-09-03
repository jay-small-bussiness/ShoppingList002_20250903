using ShoppingList002.Models.DbModels;

namespace ShoppingList002.Services
{
    public interface IActivityLogService
    {
        Task LogAsync(string actionType, int? itemId, string itemName, string categoryName, string description);
        Task InsertLogAsync(string actionType, string itemName, string categoryName, int? itemId = null);
        Task<List<ActivityLogDbModel>> GetLogsAsync();
    }

}

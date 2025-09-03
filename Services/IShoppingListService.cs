using ShoppingList002.Models.UiModels;
using ShoppingList002.Models.DbModels;

namespace ShoppingList002.Services
{
    public interface IShoppingListService
    {
        Task AddItemAsync(int itemId);
        Task AddItemsAsync(IEnumerable<CandidateListItemUiModel> items);
        Task AddToShoppingListAsync(ShoppingListItemDbModel model);
        Task CancelShoppingListItemAsync(int itemId);
        Task<List<ShoppingListUiModel>> GetDisplayItemsAsync();
        Task MarkAsPurchasedAsync(int itemId);
        Task<string?> UndoLastPurchasedItemAsync();
        Task<List<int>> GetActiveItemIdsAsync();
        Task<bool> ExistsAsync(int itemId);
    }

}

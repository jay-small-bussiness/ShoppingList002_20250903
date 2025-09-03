using ShoppingList002.Models.UiModels;
using ShoppingList002.Models.DbModels;
using ShoppingList002.Models;

namespace ShoppingList002.Services
{
    public interface ICandidateService
    {
        Task<CandidateCategoryDbModel> FindCategoryByNameAsync(string name);
        //Task<List<SearchResultItemModel>> SearchByNameAsync(string input, bool useLoosen = false);
        Task<List<SearchResultItemModel>> SearchByNameAsync(string input);
        //Task<List<SearchResultItemModel>> SearchByNameAsync(string input);
        Task<List<SearchResultItemModel>> SearchItemsAsync(string keyword);
        Task<List<CandidateCategoryDbModel>> GetCandidateCategoriesAsync();
        Task<List<CandidateListItemDbModel>> GetCandidateItemsAsync(int categoryId);
        Task<List<CandidateListItemUiModel>> GetCandidateItemsByCategoryAsync(int categoryId);
        Task<CandidateCategoryDbModel?> GetCategoryByIdAsync(int categoryId);
        Task<bool> CanDeleteCategoryAsync(int categoryId);
        Task UpdateCategoryAsync(CandidateCategoryDbModel model);
        Task<int> InsertCategoryAsync(CandidateCategoryDbModel model);
        Task<int> AddCandidateItemAsync(CandidateListItemDbModel item);
        Task DeleteCategoryAsync(int categoryId);
        //Task AddCandidateItemAsync(CandidateListItemDbModel item);
        Task<Dictionary<int, ColorSet>> GetColorMapAsync();
        Task UpdateCandidateItemAsync(CandidateListItemUiModel uiModel);
        Task DeleteCandidateItemAsync(int itemId);
        Task CopyItemToCategoryAsync(CandidateListItemUiModel sourceItem, int targetCategoryId);
        Task MoveItemToCategoryAsync(CandidateListItemUiModel sourceItem, int targetCategoryId);
    }

}

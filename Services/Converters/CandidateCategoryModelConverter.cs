using ShoppingList002.Models.DbModels;
using ShoppingList002.Models.UiModels;
using ShoppingList002.Models.JsonModels;

namespace ShoppingList002.Services.Converters
{
    public static class CandidateCategoryModelConverter
    {
        // DB → UI
        public static CandidateCategoryUiModel DbToUiModel(this CandidateCategoryDbModel dbModel)
        {
            if (dbModel == null) return null;

            return new CandidateCategoryUiModel
            {
                CategoryId = dbModel.CategoryId,
                Title = dbModel.Title,
                DisplayOrder = dbModel.DisplayOrder,
                ColorId = dbModel.ColorId,
                IconName = dbModel.IconName
            };
        }

        // UI → DB
        public static CandidateCategoryDbModel ToDbModel(this CandidateCategoryUiModel uiModel)
        {
            return new CandidateCategoryDbModel
            {
                CategoryId = uiModel.CategoryId,
                Title = uiModel.Title,
                DisplayOrder = uiModel.DisplayOrder,
                ColorId = uiModel.ColorId,
                IconName = uiModel.IconName,
                DeleteFlg = 0,
                UpdatedAt = DateTimeOffset.Now,
                IsSynced = 0
            };
        }

        // JSON → DB（初期化用）
        public static CandidateCategoryDbModel ToDbModel(this CandidateCategoryJsonModel jsonModel)
        {
            return new CandidateCategoryDbModel
            {
                CategoryId = jsonModel.CandidateListId,
                Title = jsonModel.Title,
                DisplayOrder = jsonModel.DisplayOrder,
                ColorId = jsonModel.ColorId,
                IconName = jsonModel.IconName,
                DeleteFlg = 0,
                UpdatedAt = DateTimeOffset.Now,
                IsSynced = 0
            };
        }
    }
}

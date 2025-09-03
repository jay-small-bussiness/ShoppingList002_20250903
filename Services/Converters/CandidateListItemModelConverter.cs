using ShoppingList002.Models.DbModels;
using ShoppingList002.Models.JsonModels;
using ShoppingList002.Models.UiModels;

namespace ShoppingList002.Services.Converters
{
    public static class CandidateListItemModelConverter
    {
        public static CandidateListItemDbModel FromJsonModel(CandidateListItemJsonModel json, int categoryId)
        {
            return new CandidateListItemDbModel
            {
                // ItemId はDBでAutoIncrementされる前提
                CategoryId = categoryId,
                Name = json.Name,
                Detail = json.Detail ?? "",
                DisplaySeq = json.DisplaySeq,
                DeleteFlg = 0,
                UpdatedAt = DateTimeOffset.Now,
                IsSynced = 0
            };
        }

        public static CandidateListItemUiModel FromDbModel(CandidateListItemDbModel db)
        //CandidateItemUiModel
        {
            return new CandidateListItemUiModel
            {
                ItemId = db.ItemId,
                CategoryId = db.CategoryId,
                Name = db.Name,
                Detail = db.Detail,
                DisplaySeq = db.DisplaySeq
            };
        }
        public static CandidateListItemDbModel ToDbModel(this CandidateListItemUiModel ui)
        {
            return new CandidateListItemDbModel
            {
                ItemId = ui.ItemId,
                CategoryId = ui.CategoryId,
                Name = ui.Name,
                Detail = ui.Detail,
                DisplaySeq = ui.DisplaySeq,
                DeleteFlg = 0,
                UpdatedAt = DateTimeOffset.Now,
                IsSynced = 0
            };
        }

        public static CandidateListItemUiModel DbToUiModel(this CandidateListItemDbModel dbModel)
        {
            return new CandidateListItemUiModel
            {
                ItemId = dbModel.ItemId,
                CategoryId = dbModel.CategoryId,
                Name = dbModel.Name,
                Detail = dbModel.Detail,
                DisplaySeq = dbModel.DisplaySeq
            };
        }
    }
}

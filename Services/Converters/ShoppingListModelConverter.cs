using ShoppingList002.Models.UiModels;
using ShoppingList002.Models.DbModels;

namespace ShoppingList002.Services.Converters
{
    public static class ShoppingListModelConverter
    {
        public static ShoppingListUiModel ToUiModel(
            ShoppingListItemDbModel dbModel,
            string categoryTitle,
            int categoryDisplayOrder,
            Color backgroundColor)
        {
            return new ShoppingListUiModel
            {
                ItemId = dbModel.ItemId,
                Name = dbModel.Name,
                Detail = dbModel.Detail,
                CategoryId = 0, // CategoryIdをどっかで持ってるなら渡す
                CategoryTitle = categoryTitle,
                CategoryDisplayOrder = categoryDisplayOrder,
                ItemDisplaySeq = 0, // 並び順をDB側で持つならここに反映
                BackgroundColor = backgroundColor,
                AddedDate = dbModel.AddedDate
            };
        }
    }

}

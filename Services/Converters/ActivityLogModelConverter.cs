using ShoppingList002.Models.DbModels;
using ShoppingList002.Models.UiModels;

namespace ShoppingList002.Services.Converters
{
    public static class ActivityLogModelConverter
    {
        public static ActivityLogUiModel ToUiModel(ActivityLogDbModel db)
        {
            return new ActivityLogUiModel
            {
                Timestamp = db.Timestamp.ToLocalTime().ToString("yyyy/MM/dd HH:mm"),
                ActionType = db.ActionType,
                ItemName = db.ItemName,
                CategoryName = db.CategoryName,
                Actor = db.Actor
            };
        }
    }

}

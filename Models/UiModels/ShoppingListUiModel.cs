using CommunityToolkit.Mvvm.ComponentModel;

namespace ShoppingList002.Models.UiModels
{
    public partial class ShoppingListUiModel : ObservableObject
    {
        [ObservableProperty]
        public int itemId;
        [ObservableProperty]
        public string name;
        [ObservableProperty]
        public string? detail;

        [ObservableProperty]
        public int categoryId;
        [ObservableProperty]
        public string categoryTitle;

        public int CategoryDisplayOrder { get; set; }

        public int ItemDisplaySeq { get; set; }

        public Color BackgroundColor { get; set; }

        public DateTimeOffset AddedDate { get; set; }
    }
}

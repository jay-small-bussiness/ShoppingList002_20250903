using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ShoppingList002.Models.UiModels
{
    public partial class CandidateListItemUiModel : ObservableObject
    {
        public int ItemId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryTitle { get; set; }
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string? detail;

        [ObservableProperty]
        private int displaySeq;

        [ObservableProperty]
        private int colorId;
        [ObservableProperty]
        public bool isInShoppingList;  // { get; set; } = false; // ← 追加！
        [ObservableProperty]
        private Color backgroundColor; // ← これを動的に計算して設定

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShoppingList002.Models.UiModels
{
    public class CandidateCategoryUiModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //
        private Color _backgroundColor;

        public int CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ColorId { get; set; }
        public string IconName { get; set; } = string.Empty;

        // 後で使う想定の追加プロパティ
        public int DisplayOrder { get; set; }
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        //public Color BackgroundColor { get; set; }

        // 一覧として表示するアイテム（初期は空でもOK）
        public List<CandidateListItemUiModel> Items { get; set; } = new();
        public List<ColorUiModel> ColorChoices { get; set; } = new();
        public ColorUiModel? SelectedColor { get; set; }
    }
}

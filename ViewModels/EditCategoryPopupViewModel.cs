using ShoppingList002.Models.UiModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ShoppingList002.ViewModels
{
    public class EditCategoryPopupViewModel : BaseViewModel
    {
        private readonly Guid _vmId = Guid.NewGuid();
        public string EditingTitle { get; set; } = "";
        public string EditingIcon { get; set; } = "";
        
        private ColorUiModel? _selectedColor;
        public ColorUiModel? SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    Console.WriteLine($"★★ 色選択された！ColorId: {_selectedColor?.ColorId}");
                    OnPropertyChanged();
                }
            }
        }
        public ICommand ColorTappedCommand { get; }
        private Func<CandidateCategoryUiModel, Task>? _onSaveCallback;
        private Func<CandidateCategoryUiModel, Task>? _onCancelCallback;
        public ObservableCollection<ColorUiModel> ColorOptions { get; private set; } = new();
        public CandidateCategoryUiModel? EditingCategory { get; private set; }
        //private Action<CandidateCategoryUiModel>? _onSaved;
        public Command SaveCommand { get; private set; }
        public Command CancelCommand { get; private set; }
        public EditCategoryPopupViewModel(Func<CandidateCategoryUiModel?, Task> onCompleted)
        {
            //_onCompleted = onCompleted;
            Console.WriteLine("★★ EditCategoryPopupViewModel コンストラクタ呼ばれたで！");
            SaveCommand = new Command(async () => await SaveAsync());
            CancelCommand = new Command(async () => await CancelAsync());
            ColorTappedCommand = new Command<ColorUiModel>(color =>
            {
                SelectedColor = color;
            });
        }
        public void Initialize(
            ObservableCollection<ColorUiModel> colorOptions,
            CandidateCategoryUiModel editingCategory,
            Func<CandidateCategoryUiModel, Task> onSaveCallback)
        {
            ColorOptions = new ObservableCollection<ColorUiModel>(colorOptions);
            OnPropertyChanged(nameof(ColorOptions));
            EditingTitle = editingCategory?.Title ?? string.Empty;
            EditingIcon = editingCategory?.IconName ?? string.Empty;
            SelectedColor = colorOptions.FirstOrDefault(c => c.ColorId == editingCategory?.ColorId);
            Console.WriteLine("★★ onSaveCallback null? → " + (onSaveCallback == null)); // ←これ追加！
            EditingCategory = editingCategory;
            _onSaveCallback = onSaveCallback;
            _onCancelCallback = onSaveCallback;
            Console.WriteLine($"★★ Initialize 呼び出し VM ID: {_vmId}");
        }
       
        private async Task SaveAsync()
        {
            var isNew = EditingCategory == null; // ← 新規かどうかを判定！
            Console.WriteLine($"★★ SaveAsync 実行 VM ID: {_vmId}");
            var result = new CandidateCategoryUiModel();
            try
            {
                result = new CandidateCategoryUiModel
                {
                    CategoryId = isNew ? 0 : EditingCategory.CategoryId, // ← 新規なら0で
                    //CandidateListId = category.CandidateListId, // 元のID使うなら渡しておく
                    Title = EditingTitle,
                    IconName = EditingIcon,
                    ColorId = SelectedColor?.ColorId ?? 1,
                    //IconName = "📦", // 仮
                    DisplayOrder = isNew ? -1 : EditingCategory.DisplayOrder // ← あとで決める
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save {ex}");
            }
            try
            {
                await _onSaveCallback(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save {ex}");
            }
        }

        private async Task CancelAsync()
        {
            try
            {
                await _onSaveCallback(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to cancel {ex}");
            }
        }
    }
}

using System.Collections.ObjectModel;
using System.Windows.Input;
using ShoppingList002.ViewModels;
using ShoppingList002.Models.UiModels;

namespace ShoppingList002.Views;

public partial class EditCategoryPopup : ContentPage
{
    public class EditCategoryPopupViewModel : BaseViewModel
    {
        public string EditingTitle { get; set; }
        public ObservableCollection<ColorUiModel> AvailableColors { get; } = new();
        public ColorUiModel SelectedColor { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private readonly Action<CandidateCategoryUiModel> _onSaved;

        public EditCategoryPopupViewModel(
            ObservableCollection<ColorUiModel> colorOptions,
            CandidateCategoryUiModel? editingCategory,
            Action<CandidateCategoryUiModel> onSaved)
        {
            foreach (var c in colorOptions)
                AvailableColors.Add(c);

            if (editingCategory != null)
            {
                EditingTitle = editingCategory.Title;
                SelectedColor = AvailableColors.FirstOrDefault(x => x.ColorId == editingCategory.ColorId);
            }

            _onSaved = onSaved;

            SaveCommand = new Command(() =>
            {
                var result = new CandidateCategoryUiModel
                {
                    Title = EditingTitle,
                    ColorId = SelectedColor?.ColorId ?? 0
                };
                _onSaved?.Invoke(result);
            });

            CancelCommand = new Command(async () =>
            {
                await Application.Current.MainPage.Navigation.PopModalAsync();
            });
        }
    }

}
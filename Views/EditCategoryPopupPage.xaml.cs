using CommunityToolkit.Maui.Views;
using ShoppingList002.Models.UiModels;
using ShoppingList002.ViewModels;

namespace ShoppingList002.Views;

public partial class EditCategoryPopupPage : ContentPage

{
    private readonly IServiceProvider _serviceProvider;
    private TaskCompletionSource<CandidateCategoryUiModel?> _taskSource = new();
    
    public EditCategoryPopupPage(EditCategoryPopupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    private void OnColorSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is ColorUiModel selected)
        {
            if (BindingContext is EditCategoryPopupViewModel vm)
            {
                vm.SelectedColor = selected;
            }
        }
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        if (BindingContext is EditCategoryPopupViewModel vm)
        {
            // 必要なデータ取り出して保存などやってから
             Navigation.PopModalAsync(); // ← モーダル閉じる
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        // 必要なデータ取り出して保存などやってから
         Navigation.PopModalAsync(); // ← モーダル閉じる
    }
}

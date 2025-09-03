using Microsoft.Maui.Controls;
using ShoppingList002.ViewModels;
using ShoppingList002.Services.Converters;
using ShoppingList002.Models.UiModels;
namespace ShoppingList002.Views;
[QueryProperty(nameof(CategoryId), "CategoryId")]
[QueryProperty(nameof(CategoryTitle), "CategoryTitle")]
[QueryProperty(nameof(CategoryTitleWithEmoji), "CategoryTitleWithEmoji")]

public partial class CandidateCategoryPage : ContentPage
{
    public int CategoryId { get; set; }
    public string CategoryTitle { get; set; }
    public string CategoryTitleWithEmoji { get; set; }
    public int ColorId { get; set; }

    private readonly IServiceProvider _serviceProvider;


    public CandidateCategoryPage(CandidateCategoryViewModel viewModel, IServiceProvider serviceProvider)
	{
        InitializeComponent();
        BindingContext = viewModel;
        _serviceProvider = serviceProvider;
        // ↓ここでイベントデリゲート登録するんや
        viewModel.ShowPopupRequested = async category =>
        {
            var popupVm = new EditCategoryPopupViewModel(async updated =>
            {
                var dbModel = CandidateCategoryModelConverter.ToDbModel(updated);
                await viewModel.UpdateCategoryAsync(dbModel);
            });


            popupVm.Initialize(
                viewModel.AvailableColors, // 色の選択肢
                category, // 編集中のカテゴリ（新規作成なら null でもOK）
                async updatedCategory =>
                {
                    var dbModel = CandidateCategoryModelConverter.ToDbModel(updatedCategory);
                    if (updatedCategory.CategoryId == 0)
                    {
                        // 新規追加
                        dbModel.DisplayOrder = viewModel.Categories.Count; // 最後に追加
                        dbModel.CategoryId = 0; // ←★ここで明示的に0にしとく！
                        await viewModel.InsertCategoryAsync(dbModel); // ←Insert専用メソッド
                    }
                    else
                    {
                        // 既存の編集
                        await viewModel.UpdateCategoryAsync(dbModel);
                    }
                    //await viewModel.UpdateCategoryAsync(dbModel); // VM側にUpdate処理もってるならここ
                });
            var popupPage = new EditCategoryPopupPage(popupVm);
            await Navigation.PushModalAsync(popupPage);
        };
        viewModel.IsEditMode = false;
        _ = viewModel.InitializeAsync(); // ←ここで呼ぶ！
    }
  
    private async void OnCategorySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CandidateCategoryUiModel selectedCategory)
        {
            var vm = _serviceProvider.GetService<CandidateListPageViewModel>();
            if (vm == null)
            {
                await DisplayAlert("エラー", "ViewModelがnullやったで！", "OK");
                return;
            }
            //var page = new CandidateListPage(vm, selectedCategory.CategoryId, selectedCategory.Title);
            var page = new CandidateListPage(vm);
            page.SetCategory(selectedCategory.CategoryId, selectedCategory.Title, selectedCategory.IconName, selectedCategory.ColorId);

            await Navigation.PushAsync(page);
        }


    // 選択解除（再選択できるように）
    ((CollectionView)sender).SelectedItem = null;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CandidateListPageViewModel vm)
        {
            await vm.InitializeAsync(CategoryId, CategoryTitle, CategoryTitleWithEmoji,  ColorId);
        }
    }

}
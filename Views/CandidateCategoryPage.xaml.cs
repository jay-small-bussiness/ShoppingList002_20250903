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
        // �������ŃC�x���g�f���Q�[�g�o�^������
        viewModel.ShowPopupRequested = async category =>
        {
            var popupVm = new EditCategoryPopupViewModel(async updated =>
            {
                var dbModel = CandidateCategoryModelConverter.ToDbModel(updated);
                await viewModel.UpdateCategoryAsync(dbModel);
            });


            popupVm.Initialize(
                viewModel.AvailableColors, // �F�̑I����
                category, // �ҏW���̃J�e�S���i�V�K�쐬�Ȃ� null �ł�OK�j
                async updatedCategory =>
                {
                    var dbModel = CandidateCategoryModelConverter.ToDbModel(updatedCategory);
                    if (updatedCategory.CategoryId == 0)
                    {
                        // �V�K�ǉ�
                        dbModel.DisplayOrder = viewModel.Categories.Count; // �Ō�ɒǉ�
                        dbModel.CategoryId = 0; // ���������Ŗ����I��0�ɂ��Ƃ��I
                        await viewModel.InsertCategoryAsync(dbModel); // ��Insert��p���\�b�h
                    }
                    else
                    {
                        // �����̕ҏW
                        await viewModel.UpdateCategoryAsync(dbModel);
                    }
                    //await viewModel.UpdateCategoryAsync(dbModel); // VM����Update���������Ă�Ȃ炱��
                });
            var popupPage = new EditCategoryPopupPage(popupVm);
            await Navigation.PushModalAsync(popupPage);
        };
        viewModel.IsEditMode = false;
        _ = viewModel.InitializeAsync(); // �������ŌĂԁI
    }
  
    private async void OnCategorySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CandidateCategoryUiModel selectedCategory)
        {
            var vm = _serviceProvider.GetService<CandidateListPageViewModel>();
            if (vm == null)
            {
                await DisplayAlert("�G���[", "ViewModel��null������ŁI", "OK");
                return;
            }
            //var page = new CandidateListPage(vm, selectedCategory.CategoryId, selectedCategory.Title);
            var page = new CandidateListPage(vm);
            page.SetCategory(selectedCategory.CategoryId, selectedCategory.Title, selectedCategory.IconName, selectedCategory.ColorId);

            await Navigation.PushAsync(page);
        }


    // �I�������i�đI���ł���悤�Ɂj
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
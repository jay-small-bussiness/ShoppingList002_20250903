using ShoppingList002.ViewModels;
using ShoppingList002.Models.UiModels;
using CommunityToolkit.Maui.Views;
namespace ShoppingList002.Views;
[QueryProperty(nameof(CategoryId), "categoryId")]
[QueryProperty(nameof(CategoryTitle), "categoryTitle")]
[QueryProperty(nameof(CategoryTitleWithEmoji), "CategoryTitleWithEmoji")]
[QueryProperty(nameof(ColorId), "colorId")]
[QueryProperty(nameof(FromVoice), "fromVoice")]
public partial class CandidateListPage : ContentPage
{
    public string FromVoice { get; set; }

    public int CategoryId { get; set; }
    public string CategoryTitle { get; set; }
    public string CategoryTitleWithEmoji { get; set; }
    public int ColorId { get; set; }

    private readonly CandidateListPageViewModel _viewModel;

//    public CandidateListPage(CandidateListPageViewModel viewModel, int categoryId, string categoryTitle)
    public CandidateListPage(CandidateListPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        //Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });

        //_ = viewModel.InitializeAsync(categoryId, categoryTitle);
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (FromVoice == "true")
        {
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = false });
        }
        else
        {
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior { IsVisible = true });
        }
    }
    public void SetCategory(int categoryId, string categoryTitle, string categoryTitleWithEmoji, int colorId)
    {
        _viewModel.InitializeAsync(categoryId, categoryTitle, categoryTitleWithEmoji, colorId); // ← 非同期でも非awaitでOK
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CandidateListPageViewModel vm)
        {
            await vm.InitializeAsync(CategoryId, CategoryTitle, CategoryTitleWithEmoji, ColorId);
        }
    }
    private async void OnAddClicked(object sender, EventArgs e)
    {
        var popup = new AddCandidateItemPopup();
        var result = await this.ShowPopupAsync(popup);

        if (result is not null)
        {
            var name = result.GetType().GetProperty("Name")?.GetValue(result)?.ToString();
            var detail = result.GetType().GetProperty("Detail")?.GetValue(result)?.ToString();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var vm = BindingContext as CandidateListPageViewModel;
                if (vm != null)
                    await vm.AddItemFromPopupAsync(name, detail);
            }
            else
            {
                await DisplayAlert("エラー", "名前は必須です！", "OK");
            }
        }
    }
  
}
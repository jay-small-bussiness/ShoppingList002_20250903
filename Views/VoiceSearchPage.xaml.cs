//using AndroidX.Lifecycle;
using ShoppingList002.ViewModels;
using ShoppingList002.Models.UiModels;
namespace ShoppingList002.Views;

public partial class VoiceSearchPage : ContentPage
{
    private readonly VoiceSearchViewModel _viewModel;

    public VoiceSearchPage(VoiceSearchViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.StartListeningAsync();
    }
    private async void OnItemTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is SearchResultItemModel selectedItem)
        {
            if (BindingContext is VoiceSearchViewModel vm)
            {
                await vm.OnItemSelectedAsync(selectedItem);
            }
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is VoiceSearchViewModel vm)
            vm.StopListeningCommand?.Execute(null);
    }

    //private async void OnItemSelected(object sender, SelectionChangedEventArgs e)
    //{
    //    if (e.CurrentSelection.FirstOrDefault() is SearchResultItemModel selectedItem)
    //    {
    //        if (BindingContext is VoiceSearchViewModel vm)
    //        {
    //            await vm.OnItemSelectedAsync(selectedItem);

    //            // ✅ 追加：選択後にリストを空に（または選択解除）
    //            ((CollectionView)sender).SelectedItem = null;
    //        }
    //    }
    //}

    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();

    //    if (BindingContext is VoiceSearchViewModel vm)
    //    {
    //        await vm.StartListeningAsync(); // 直接呼びもOKやで
    //    }
    //}
}
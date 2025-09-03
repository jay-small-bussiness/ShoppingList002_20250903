using ShoppingList002.Services;
using ShoppingList002.ViewModels;
using ShoppingList002.Models.UiModels;

namespace ShoppingList002.Views;

public partial class ShoppingListPage : ContentPage
{
    private readonly ShoppingListPageViewModel _viewModel;
    private TutorialManager _tutorialManager;

    public ShoppingListPage(ShoppingListPageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshAsync(); // ←後述
        _tutorialManager = new TutorialManager(new List<TutorialStep>
        {
            new TutorialStep
            {
                PreferenceKey = "HasSeenSLStep1",
                TutorialString = "最初にこのボタンを\n押してみよう！\n👇",
                FontSize = 18,
                TextColor = Colors.DarkRed,
                HorizontalAlignment = LayoutOptions.Center,
                VerticalAlignment = LayoutOptions.End,
                OverlayGrid = TutorialOverlaySL01
            }
        });
        _tutorialManager.Start();
        //TutorialOverlaySL01.IsVisible = true;
    }
    private void OnItemTapped(object sender, EventArgs e)
    {
        _tutorialManager?.CompleteCurrentStep();
    }

    private async void OnGoToCandidatesClicked(object sender, EventArgs e)
    {
        Preferences.Set("HasSeenTutorialStep1", true);
        //TutorialHint.IsVisible = false;
        TutorialOverlaySL01.IsVisible = false;
        _tutorialManager.CompleteCurrentStep();
        // ↓実際の遷移処理に書き換え
        //await Shell.Current.GoToAsync("CandidateCategoryPage");
    }
}
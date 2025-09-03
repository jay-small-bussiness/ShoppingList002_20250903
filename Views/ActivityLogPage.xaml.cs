using ShoppingList002.ViewModels;
namespace ShoppingList002.Views;

public partial class ActivityLogPage : ContentPage
{
    public ActivityLogPage(ActivityLogPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
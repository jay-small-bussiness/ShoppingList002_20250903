using ShoppingList002.Models.UiModels;
using CommunityToolkit.Maui.Views;

namespace ShoppingList002.Views;

public partial class SelectCategoryPopup : Popup
{
    public List<CandidateCategoryUiModel> Categories { get; set; }
    private TaskCompletionSource<CandidateCategoryUiModel?> _tcs = new();

    public SelectCategoryPopup(List<CandidateCategoryUiModel> categories)
    {
        InitializeComponent();
        Categories = categories;
        BindingContext = this;
    }

    public Task<CandidateCategoryUiModel?> GetSelectedCategoryAsync() => _tcs.Task;

    private void OnCancelClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(null);
        this.Close();
    }

    private void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is CandidateCategoryUiModel category && _tcs != null)
        {
            _tcs.TrySetResult(category);
            this.Close();
        }
        else
        {
            Console.WriteLine("⚠️ Category tap failed: Parameter or _tcs is null");
        }
    }
}

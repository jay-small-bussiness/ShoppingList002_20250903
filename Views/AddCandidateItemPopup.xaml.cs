using CommunityToolkit.Maui.Views;

namespace ShoppingList002.Views;

public partial class AddCandidateItemPopup : Popup
{
    public AddCandidateItemPopup()
    {
        InitializeComponent();
    }
    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }
    private void OnAddClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        var detail = DetailEntry.Text?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            // バリデーション：空はNG
            return;
        }

        Close(new CandidateItemInputResult
        {
            Name = name,
            Detail = detail
        });
    }
    public class CandidateItemInputResult
    {
        public string Name { get; set; } = "";
        public string? Detail { get; set; }
    }
}
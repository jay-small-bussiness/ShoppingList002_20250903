using ShoppingList002.Models.UiModels;
using CommunityToolkit.Maui.Views;

namespace ShoppingList002.Views;

public partial class EditCandidateItemPopup : Popup
{
    public CandidateListItemUiModel EditableItem { get; private set; }

    public EditCandidateItemPopup(CandidateListItemUiModel item)
    {
        InitializeComponent();

        // コピーして編集用に（元のリストと分離）
        EditableItem = new CandidateListItemUiModel
        {
            ItemId = item.ItemId,
            CategoryId = item.CategoryId,
            Name = item.Name,
            Detail = item.Detail,
            DisplaySeq = item.DisplaySeq,
            ColorId = item.ColorId
        };

        BindingContext = EditableItem;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        this.Close();
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        MessagingCenter.Send(this, "EditItemConfirmed", EditableItem);
        this.Close();
    }

    private void OnDeleteClicked(object sender, EventArgs e)
    {
        MessagingCenter.Send(this, "DeleteItemConfirmed", EditableItem);
        this.Close();
    }
}

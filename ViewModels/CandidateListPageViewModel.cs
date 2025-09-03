using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ShoppingList002.Models.UiModels;
using ShoppingList002.Models.DbModels;
using ShoppingList002.Services;
using ShoppingList002.Models;
using ShoppingList002.Views;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using ShoppingList002.Services.Converters;

//using CoreImage;


namespace ShoppingList002.ViewModels
{
    public partial class CandidateListPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isEditMode;
        [ObservableProperty]
        private string categoryTitle;
        [ObservableProperty]
        private string categoryTitleWithEmoji;
        [ObservableProperty]
        private int colorId;

        private bool _isRefreshing = false;
        private readonly ICandidateService _candidateService;
        private readonly IShoppingListService _shoppingListService;
        private readonly IDatabaseService _databaseService;
        private readonly ActivityLogService _activityLogService;

        //private string _categoryTitle = string.Empty;
        private Dictionary<int, ColorSet> _colorMap = new();
        public IRelayCommand GoToShoppingListCommand { get; }
        public IRelayCommand ToggleEditModeCommand { get; }
        public IRelayCommand<CandidateListItemUiModel> MoveItemUpCommand { get; }
        public IRelayCommand<CandidateListItemUiModel> MoveItemDownCommand { get; }
        public IRelayCommand<CandidateListItemUiModel> ShowItemMenuCommand { get; }

        public string EditButtonText => IsEditMode ? "🛑 編集モード終了" : "✏️ 編集モードへ";
        public int CategoryID;

        public ObservableCollection<CandidateListItemUiModel> Items { get; } = new();
        public IAsyncRelayCommand<CandidateListItemUiModel> OnItemTappedCommand { get; }

        public ICommand AddToShoppingListCommand { get; }

        public CandidateListPageViewModel(ICandidateService candidateService
                                        , IShoppingListService shoppingListService
                                        , IDatabaseService databaseService
                                        , ActivityLogService activityLogService)
        {
            _candidateService = candidateService;
            _shoppingListService = shoppingListService;
            _databaseService = databaseService;
            _activityLogService = activityLogService;

            OnItemTappedCommand = new AsyncRelayCommand<CandidateListItemUiModel>(OnItemTappedAsync);
            GoToShoppingListCommand = new RelayCommand(async () =>
            {
                await Shell.Current.GoToAsync("///ShoppingListPage");
            });
            ToggleEditModeCommand = new RelayCommand(() =>
            {
                IsEditMode = !IsEditMode;
                OnPropertyChanged(nameof(EditButtonText));
            });
            MoveItemUpCommand = new RelayCommand<CandidateListItemUiModel>(MoveItemUp);
            MoveItemDownCommand = new RelayCommand<CandidateListItemUiModel>(MoveItemDown);
            ShowItemMenuCommand = new RelayCommand<CandidateListItemUiModel>(ShowItemMenu);
            MessagingCenter.Subscribe<EditCandidateItemPopup, CandidateListItemUiModel>(this, "EditItemConfirmed", async (sender, editedItem) =>
            {
                // DB更新
                await _candidateService.UpdateCandidateItemAsync(editedItem);

                // UIモデル側の更新
                var target = Items.FirstOrDefault(x => x.ItemId == editedItem.ItemId);
                if (target != null)
                {
                    target.Name = editedItem.Name;
                    target.Detail = editedItem.Detail;
                    target.DisplaySeq = editedItem.DisplaySeq;

                    // もし並び順に関係するなら、ItemsのSortし直しもここで
                }
            });

            MessagingCenter.Subscribe<EditCandidateItemPopup, CandidateListItemUiModel>(this, "DeleteItemConfirmed", async (sender, deletedItem) =>
            {
                // DB削除（ソフト or ハードどっちでもOK）
                await _candidateService.DeleteCandidateItemAsync(deletedItem.ItemId);

                // UI上からも除外
                var target = Items.FirstOrDefault(x => x.ItemId == deletedItem.ItemId);
                if (target != null)
                {
                    Items.Remove(target);
                }
            });
        }
        
        private async void ShowItemMenu(CandidateListItemUiModel item)
        {
            string action = await Shell.Current.DisplayActionSheet(
                "この項目を",
                "キャンセル",
                null,
                "編集する",
                "コピーする",
                "移動する",
                "削除する");

            if (action == "コピーする")
            {
                await SelectCategoryAndCopyAsync(item);
            }
            else if (action == "編集する")
            {
                // 編集モード中：編集用ポップアップを開く
                var popup = new EditCandidateItemPopup(item);
                try
                {
                    await Application.Current.MainPage.ShowPopupAsync(popup);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else if (action == "移動する")
            {
                await SelectCategoryAndMoveAsync(item);
            }
            else if (action == "削除する")
            {
                bool isInShoppingList = await _shoppingListService.ExistsAsync(item.ItemId);
                if (isInShoppingList)
                {
                    await Shell.Current.DisplayAlert(
                        "注意",
                        "このアイテムは現在、買い物リストに入っています。削除できません",
                        "OK");
                    return;
                    // DB削除（ソフト or ハードどっちでもOK）
                }
                bool proceed = await Shell.Current.DisplayAlert(
                    "注意",
                    "本当に削除しますか",
                    "OK", "キャンセル");

                if (!proceed)
                    return;
                // DB削除（ソフト or ハードどっちでもOK）
                await _candidateService.DeleteCandidateItemAsync(item.ItemId);

                // UI上からも除外
                var target = Items.FirstOrDefault(x => x.ItemId == item.ItemId);
                if (target != null)
                {
                    Items.Remove(target);
                }
            }
        }
        private async Task LogItemMovedAsync(string itemName, string oldCategory, string newCategory, int itemId)
        {
            await _activityLogService.InsertLogAsync(
                actionType: "アイテム移動",
                itemName: itemName,
                categoryName: $"{oldCategory} → {newCategory}",
                itemId: itemId
            );
        }
        private async Task LogItemAddAsync(string itemName, int itemId, string categoryName)
        {
            await _activityLogService.InsertLogAsync(
                actionType: "リスト追加",
                itemName: itemName,
                categoryName: categoryName,
                itemId: itemId
            );
        }
        private async Task LogItemDelAsync(string itemName, int itemId, string categoryName)
        {
            await _activityLogService.InsertLogAsync(
                actionType: "リスト削除",
                itemName: itemName,
                categoryName: categoryName,
                itemId: itemId
            );
        }
        private async Task SelectCategoryAndCopyAsync(CandidateListItemUiModel item)
        {
            var dbModels = await _candidateService.GetCandidateCategoriesAsync(); // UIモデルで受け取れる形式
            var uiModels = dbModels.Select(x => x.DbToUiModel()).ToList();

            var popup = new SelectCategoryPopup(uiModels);

            await Shell.Current.CurrentPage.ShowPopupAsync(popup);
            var category = await popup.GetSelectedCategoryAsync();

            if (category == null)
                return;

            await _candidateService.CopyItemToCategoryAsync(item, category.CategoryId);

            //await RefreshAsync(); // 必要なら再読み込み
        }
        private async Task SelectCategoryAndMoveAsync(CandidateListItemUiModel item)
        {
            var dbModels = await _candidateService.GetCandidateCategoriesAsync();
            var uiModels = dbModels.Select(x => x.DbToUiModel()).ToList();

            var popup = new SelectCategoryPopup(uiModels);
            await Shell.Current.CurrentPage.ShowPopupAsync(popup);
            var category = await popup.GetSelectedCategoryAsync();

            if (category == null)
                return;

            // ✅ ShoppingListにあるかチェック（必要なら _shoppingService に確認）
            bool isInShoppingList = await _shoppingListService.ExistsAsync(item.ItemId);
            if (isInShoppingList)
            {
                bool proceed = await Shell.Current.DisplayAlert(
                    "注意",
                    "このアイテムは現在、買い物リストに入っています。移動するとリストからも削除されます。よろしいですか？",
                    "OK", "キャンセル");

                if (!proceed)
                    return;
            }

            await _candidateService.MoveItemToCategoryAsync(item, category.CategoryId);
            await LogItemMovedAsync(item.Name, categoryTitle, category.Title, item.ItemId);

            await RefreshAsync(); // 必要なら再読み込み
        }

        public void AddItemFromPopup(string name, string? detail)
        {
            var newItem = new CandidateListItemUiModel
            {
                ItemId = 0, // ← DB未登録なので仮
                Name = name,
                Detail = detail,
                DisplaySeq = Items.Count + 1,
                CategoryId = CategoryID,
                ColorId = ColorId,
                IsInShoppingList = false,
                BackgroundColor = _colorMap.TryGetValue(ColorId, out var colorSet)
                    ? colorSet.Unselected
                    : Colors.Transparent
            };

            Items.Add(newItem);
        }


        public async Task OnItemTappedAsync(CandidateListItemUiModel item)
        {
            if (IsEditMode)
            {
            }
            else
            {
                if (item.IsInShoppingList)
                {
                    // すでに選択済 → キャンセル（ShoppingListから削除）
                    await _shoppingListService.CancelShoppingListItemAsync(item.ItemId);
                    item.IsInShoppingList = false;
                    //Logに残す
                    await LogItemDelAsync(item.Name, item.ItemId, CategoryTitle);

                    if (_colorMap.TryGetValue(item.ColorId, out var colorSet))
                        item.BackgroundColor = colorSet.Unselected;
                }
                else
                {
                    //+++++++++++++++++++++++++++++
                    // 未選択 → 追加
                    await _shoppingListService.AddToShoppingListAsync(new ShoppingListItemDbModel
                    {
                        ItemId = item.ItemId,
                        Name = item.Name,
                        Detail = item.Detail,
                        AddedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        Status = null
                    });
                    //Logに残す
                    await LogItemAddAsync(item.Name, item.ItemId, CategoryTitle);
                    item.IsInShoppingList = true;

                    if (_colorMap.TryGetValue(item.ColorId, out var colorSet))
                        item.BackgroundColor = colorSet.Selected;
                }
            }
        }

       
        public async Task InitializeAsync(int categoryId, string categoryTitle, string categoryTitleWithEmoji, int colorId)
        {
            CategoryTitle = categoryTitle;
            CategoryID = categoryId;
            CategoryTitleWithEmoji = categoryTitleWithEmoji;
            ColorId = colorId;
      
            _colorMap = await _databaseService.GetColorSetMapAsync();
            await RefreshAsync(); // ←ここだけで Add される
        }
        public async Task RefreshAsync()
        {
            if (_isRefreshing)
                return;

            _isRefreshing = true;
            try
            {
                Items.Clear();
                var newItems = await LoadCandidateItemsWithStateAsync();
                foreach (var item in newItems)
                {
                    Items.Add(item);
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }
        private async Task<List<CandidateListItemUiModel>> LoadCandidateItemsWithStateAsync()
        {
            var itemList = await _candidateService.GetCandidateItemsByCategoryAsync(CategoryID);
            var shoppingItemIds = await _shoppingListService.GetActiveItemIdsAsync();
            var allCategories = await _databaseService.GetAllAsync<CandidateCategoryDbModel>();
            var colorMap = await _databaseService.GetColorSetMapAsync();

            var uiList = new List<CandidateListItemUiModel>();

            foreach (var item in itemList)
            {
                var category = allCategories.FirstOrDefault(x => x.CategoryId == item.CategoryId);
                var ui = new CandidateListItemUiModel
                {
                    ItemId = item.ItemId,
                    Name = item.Name,
                    Detail = item.Detail,
                    DisplaySeq = item.DisplaySeq,
                    CategoryId = item.CategoryId,
                    ColorId = category?.ColorId ?? 0,
                    IsInShoppingList = shoppingItemIds.Contains(item.ItemId),
                    BackgroundColor = (category != null && colorMap.TryGetValue(category.ColorId, out var colorSet))
                        ? (shoppingItemIds.Contains(item.ItemId) ? colorSet.Selected : colorSet.Unselected)
                        : Colors.Transparent
                };

                uiList.Add(ui);
            }

            return uiList.OrderBy(x => x.DisplaySeq).ToList();
        }
        private void MoveItemUp(CandidateListItemUiModel item)
        {
            var index = Items.IndexOf(item);
            if (index > 0)
            {
                var above = Items[index - 1];

                (item.DisplaySeq, above.DisplaySeq) = (above.DisplaySeq, item.DisplaySeq);

                ResortItems();
            }
        }
        private void MoveItemDown(CandidateListItemUiModel item)
        {
            var index = Items.IndexOf(item);
            if (index < Items.Count - 1)
            {
                var below = Items[index + 1];

                (item.DisplaySeq, below.DisplaySeq) = (below.DisplaySeq, item.DisplaySeq);

                ResortItems();
            }
        }
        private void ResortItems()
        {
            var sorted = Items.OrderBy(x => x.DisplaySeq).ToList();
            Items.Clear();
            foreach (var item in sorted)
                Items.Add(item);
        }
        partial void OnIsEditModeChanged(bool oldValue, bool newValue)
        {
            if (!newValue) // 編集モード → 通常モード に戻ったとき
            {
                _ = SaveDisplayOrderAsync();
                //SaveDisplayOrderAsync().FireAndForget();
            }
        }
        private async Task SaveDisplayOrderAsync()
        {
            int seq = 1;
            foreach (var item in Items.OrderBy(x => x.DisplaySeq))
            {
                item.DisplaySeq = seq++;
                await _candidateService.UpdateCandidateItemAsync(item);
            }
        }

        public async Task AddItemFromPopupAsync(string name, string? detail)
        {
            var newItem = new CandidateListItemUiModel
            {
                ItemId = 0, // 仮
                Name = name,
                Detail = detail,
                DisplaySeq = Items.Count + 1,
                CategoryId = CategoryID,
                ColorId = ColorId,
                IsInShoppingList = false,
                BackgroundColor = _colorMap.TryGetValue(ColorId, out var colorSet)
                    ? colorSet.Unselected
                    : Colors.Transparent
            };
            // DBに保存
            var dbModel = new CandidateListItemDbModel
            {
                CategoryId = newItem.CategoryId,
                ItemId = 0,
                Name = newItem.Name,
                Detail = newItem.Detail,
                DisplaySeq = newItem.DisplaySeq,
                //ColorId = newItem.ColorId,
                UpdatedAt = DateTimeOffset.Now,
                DeleteFlg = 0
            };
       
        //await _candidateService.AddCandidateItemAsync(dbModel);
        var newItemId = await _candidateService.AddCandidateItemAsync(dbModel);
        newItem.ItemId = newItemId;

        Items.Add(newItem);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}

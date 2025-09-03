using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingList002.Models.UiModels;
using ShoppingList002.Models.DbModels;
using ShoppingList002.Services;
using ShoppingList002.Exceptions;
using ShoppingList002.Views;
using ShoppingList002.Services.Converters;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;

namespace ShoppingList002.ViewModels
{
    public partial class CandidateCategoryViewModel : ObservableObject
    {
        private readonly ICandidateService _candidateService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private bool isEditMode;
        public ObservableCollection<CandidateCategoryUiModel> Categories { get; set; } = new();
        public ObservableCollection<ColorUiModel> AvailableColors { get; } = new();

        public int SelectedColorId { get; set; }
        //public Func<Task>? ShowPopupRequested { get; set; }

        public ICommand ToggleEditModeCommand { get; }
        public ICommand DeleteCategoryCommand => new AsyncRelayCommand<int>(OnDeleteCategory);
        //public ICommand ShowEditCategoryPopupCommand { get; }
        public ICommand ShowCategoryMenuCommand { get; }
        public ICommand MoveItemUpCommand { get; }
        public ICommand MoveItemDownCommand { get; }
        public ICommand AddNewCategoryCommand { get; }
        //public Action<CandidateCategoryUiModel>? ShowPopupRequested { get; set; }

        //// View 側でこれに代入してもらう
        public Func<CandidateCategoryUiModel, Task>? ShowPopupRequested { get; set; }


        public RelayCommand<CandidateCategoryUiModel> CategoryTappedCommand { get; }
        //public RelayCommand<CandidateCategoryUiModel> ShowCategoryMenuCommand { get; }

        public string EditButtonText => IsEditMode ? "🛑 編集モード終了" : "✏️ 編集モードへ";
        public CandidateCategoryViewModel(ICandidateService candidateService, IServiceProvider serviceProvider)
        {
            _candidateService = candidateService;
            _serviceProvider = serviceProvider;
            AddNewCategoryCommand = new RelayCommand(AddNewCategory);

            CategoryTappedCommand = new RelayCommand<CandidateCategoryUiModel>(async category =>
            {
                if (IsEditMode)
                    return; // 編集モード中は無視
                await OnCategoryTappedAsync(category);
            });
            
            ToggleEditModeCommand = new AsyncRelayCommand(async () =>
            {
                IsEditMode = !IsEditMode;
                OnPropertyChanged(nameof(EditButtonText));
                if (!IsEditMode)
                {
                    await SaveCategoryOrderAsync();
                }
            });
            ShowCategoryMenuCommand = new RelayCommand<CandidateCategoryUiModel>(async category =>
            {
                var title = $"カテゴリ編集（{category.Title}）";

                var action = await Shell.Current.DisplayActionSheet(
                    title, "キャンセル", null,
                    "名前・色を編集", "カテゴリを削除");

                if (action == "名前・色を編集")
                {
                    ShowPopupRequested.Invoke(category);
                }
                else if (action == "カテゴリを削除")
                {
                    // ← ここでその場チェック！
                    var items = await _candidateService.GetCandidateItemsByCategoryAsync(category.CategoryId);
                    if (items.Any())
                    {
                        await Shell.Current.DisplayAlert(
                            "削除できません",
                            "このカテゴリーには項目が含まれています。\n\n「カテゴリー内の編集」で、すべて移動または削除してください。",
                            "OK");
                        return;
                    }

                    await DeleteCategoryAsync(category);
                }
            });

            MoveItemUpCommand = new Command<CandidateCategoryUiModel>(MoveItemUp);
            MoveItemDownCommand = new Command<CandidateCategoryUiModel>(MoveItemDown);
        }
        private void AddNewCategory()
        {
            ShowPopupRequested?.Invoke(null); // nullで新規追加扱い
        }

        //private async Task ShowEditCategoryPopup(CandidateCategoryUiModel category)
        //{
        //    await ShowPopupRequested.Invoke(category);
        //}
        private void MoveItemUp(CandidateCategoryUiModel item)
        {
            var index = Categories.IndexOf(item);
            if (index > 0)
            {
                var above = Categories[index - 1];
                (item.DisplayOrder, above.DisplayOrder) = (above.DisplayOrder, item.DisplayOrder);
                ResortItems();
            }
        }

        private void MoveItemDown(CandidateCategoryUiModel item)
        {
            var index = Categories.IndexOf(item);
            if (index < Categories.Count - 1)
            {
                var below = Categories[index + 1];
                (item.DisplayOrder, below.DisplayOrder) = (below.DisplayOrder, item.DisplayOrder);
                ResortItems();
            }
        }
        private void ResortItems()
        {
            var sorted = Categories.OrderBy(x => x.DisplayOrder).ToList();
            Categories.Clear();
            foreach (var item in sorted)
                Categories.Add(item);
        }
       
        private async Task DeleteCategoryAsync(CandidateCategoryUiModel category)
        {
            bool confirm = await Shell.Current.DisplayAlert("確認", $"{category.Title} を削除しますか？", "はい", "いいえ");
            if (confirm)
            {
                await _candidateService.DeleteCategoryAsync(category.CategoryId);
                await InitializeAsync(); // リスト再読み込み
            }
        }

        private async Task OnCategoryTappedAsync(CandidateCategoryUiModel category)
        {
            if (IsEditMode)
            {
                // 編集モード中は無視（あるいは逆に編集画面に行くとか）
                Debug.WriteLine("🛑 編集モード中なので選択処理スキップ");
                return;
            }
            Console.WriteLine($"📂 カテゴリ選択: {category.Title}");

            // 遷移処理へ
            //await Shell.Current.GoToAsync($"candidatelist?categoryId={category.CategoryId}&categoryTitle={category.Title}&colorId={category.ColorId}");
            await Shell.Current.GoToAsync($"candidatelist?categoryId={category.CategoryId}&categoryTitle={category.Title}&CategoryTitleWithEmoji={category.IconName}&colorId={category.ColorId}");

        }
    
        public async Task InitializeAsync()
        {
            var dbModels = await _candidateService.GetCandidateCategoriesAsync();
            var colorMap = await _candidateService.GetColorMapAsync(); // 追加（候補アイテムの色と共通）
            Categories.Clear();
            foreach (var db in dbModels)
            {
                var ui = db.DbToUiModel(); // ← ここで IconName, ColorId, Title は入ってる前提

                if (colorMap.TryGetValue(ui.ColorId, out var colorSet))
                {
                    ui.BackgroundColor = colorSet.Unselected;
                }
                Categories.Add(ui);
            }
            OnPropertyChanged(nameof(Categories));

            AvailableColors.Clear();
            foreach (var (id, colorSet) in colorMap)
            {
                AvailableColors.Add(new ColorUiModel
                {
                    ColorId = id,
                    Name = $"色{id}",
                    ColorValue = colorSet.Unselected
                });
            }
            OnPropertyChanged(nameof(AvailableColors));

        }
        public async void ShowCategoryMenu(CandidateCategoryUiModel category)
        {
            Console.WriteLine("★ ShowCategoryMenu called");
            await Shell.Current.DisplayAlert("確認", $"カテゴリ: {category?.Title}", "OK");
            string action = await Shell.Current.DisplayActionSheet(
                "このカテゴリを",
                "キャンセル",
                null,
                "名前を変更する",
                "削除する");

            if (action == "名前を変更する")
            {
                // TODO: 名前変更処理
            }
            else if (action == "並び順を変更する")
            {
                // TODO: 並び順変更処理
            }
            else if (action == "削除する")
            {
                await OnDeleteCategory(category.CategoryId);
            }
        }
        public async Task UpdateCategoryAsync(CandidateCategoryDbModel dbModel)
        {
            await _candidateService.UpdateCategoryAsync(dbModel);

            // 一覧に反映させるため、更新後のデータを読み直す
            await InitializeAsync();
        }
        public async Task InsertCategoryAsync(CandidateCategoryDbModel dbModel)
        {
            var colorMap = await _candidateService.GetColorMapAsync(); // 追加（候補アイテムの色と共通）

            var newId = await _candidateService.InsertCategoryAsync(dbModel);
            var uiModel = CandidateCategoryModelConverter.DbToUiModel(dbModel);
            //uiModel.CategoryId = 0;
            // 背景色はここで設定してるはず 
            if (colorMap.TryGetValue(uiModel.ColorId, out var colorSet))
            {
                uiModel.BackgroundColor = colorSet.Unselected; // ← この時点で通知されるように！
            }
            Categories.Add(uiModel);
        }
        public async Task SaveCategoryOrderAsync()
        {
            foreach (var (category, index) in Categories.Select((c, i) => (c, i)))
            {
                category.DisplayOrder = index; // or SortOrder
                var dbModel = CandidateCategoryModelConverter.ToDbModel(category);
                await _candidateService.UpdateCategoryAsync(dbModel);
            }
        }
        private async Task OnDeleteCategory(int categoryId)
        {
            try
            {
                var ok = await _candidateService.CanDeleteCategoryAsync(categoryId);
                if (ok)
                {
                    await _candidateService.DeleteCategoryAsync(categoryId);
                    await InitializeAsync(); // カテゴリ再読込など
                }
            }
            catch (CategoryNotEmptyException ex)
            {
                var msg = $"このカテゴリには {ex.AllItems.Count} 件のアイテムが残っています。";

                if (ex.ActiveShoppingItems.Any())
                {
                    msg += "\n\n以下のアイテムは現在買い物リストにあります：\n";
                    msg += string.Join("\n", ex.ActiveShoppingItems.Select(name => $"・{name}"));
                }

                await Shell.Current.DisplayAlert("削除できません", msg, "OK");
            }
        }
    }


}

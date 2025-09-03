using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingList002.Models.UiModels;
using ShoppingList002.Models.DbModels;
using ShoppingList002.Services;
using System.Collections.ObjectModel;

namespace ShoppingList002.ViewModels;

public partial class ActivityLogPageViewModel : ObservableObject
{
    private readonly ActivityLogService _activityLogService;

    public ObservableCollection<ActivityLogUiModel> FilteredLogs { get; } = new();

    public List<string> PeriodOptions { get; } = new() { "1週間", "2週間", "1か月", "3か月" };

    [ObservableProperty]
    private string selectedPeriod;

    [ObservableProperty]
    private bool addSelected = true;

    [ObservableProperty]
    private bool deleteSelected = true;

    [ObservableProperty]
    private bool purchasedSelected = true;

    [RelayCommand]
    private void ToggleAdd() => AddSelected = !AddSelected;

    [RelayCommand]
    private void ToggleDelete() => DeleteSelected = !DeleteSelected;

    [RelayCommand]
    private void TogglePurchased() => PurchasedSelected = !PurchasedSelected;

    [RelayCommand]
    public async Task LoadLogsAsync()
    {
        var allLogs = await _activityLogService.GetLogsAsync();
        //ApplyFilter(allLogs);
        LoadAndFilterLogsAsync();
    }

    partial void OnAddSelectedChanged(bool value) => LoadLogsCommand.Execute(null);
    partial void OnDeleteSelectedChanged(bool value) => LoadLogsCommand.Execute(null);
    partial void OnPurchasedSelectedChanged(bool value) => LoadLogsCommand.Execute(null);
    partial void OnSelectedPeriodChanged(string value) => LoadLogsCommand.Execute(null);
    private async Task LoadAndFilterLogsAsync()
    {
        var logs = await _activityLogService.GetLogsAsync();
        var uiLogs = logs
            .Where(log => (AddSelected && log.ActionType == "リスト追加") ||
                          (DeleteSelected && log.ActionType == "リスト削除") ||
                          (PurchasedSelected && log.ActionType == "購入"))
            .Select(log => new ActivityLogUiModel
            {
                Timestamp = log.Timestamp.ToOffset(TimeSpan.FromHours(9)).ToString("MM/dd HH:mm"),
                ActionType = log.ActionType,
                ItemName = log.ItemName,
                CategoryName = $"[{log.CategoryName}]",
                Actor = log.Actor
            })
            .ToList();

        FilteredLogs.Clear();
        foreach (var item in uiLogs)
            FilteredLogs.Add(item);
    }
    private async Task ToggleAddAsync()
{
    AddSelected = !AddSelected;
    await LoadAndFilterLogsAsync();
}

    private void ApplyFilter(List<ActivityLogDbModel> allLogs)
    {
        FilteredLogs.Clear();

        // フィルター条件
        var periodDays = SelectedPeriod switch
        {
            "1週間" => 7,
            "2週間" => 14,
            "1か月" => 30,
            "3か月" => 90,
            _ => 30
        };

        var cutoff = DateTimeOffset.Now.AddDays(-periodDays);

        foreach (var log in allLogs)
        {
            if (log.Timestamp < cutoff)
                continue;

            if ((AddSelected && log.ActionType == "追加") ||
                (DeleteSelected && log.ActionType == "削除") ||
                (PurchasedSelected && log.ActionType == "購入"))
            {
                FilteredLogs.Add(new ActivityLogUiModel
                {
                    Timestamp = log.Timestamp.ToString("yyyy/MM/dd HH:mm"),
                    ActionType = log.ActionType,
                    ItemName = log.ItemName,
                    CategoryName = log.CategoryName,
                    Actor = log.Actor
                });
            }
        }
    }

    public ActivityLogPageViewModel(ActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
        SelectedPeriod = PeriodOptions.First();
    }
}

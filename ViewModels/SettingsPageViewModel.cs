// SettingsPageViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ShoppingList002.Services;
using ShoppingList002.Views;
namespace ShoppingList002.ViewModels;

public partial class SettingsPageViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    [ObservableProperty]
    private string selectedRetention;

    public SettingsPageViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        SelectedRetention = _settingsService.LoadRetentionPeriod();
    }

    [RelayCommand]
    private async Task Save()
    {
        _settingsService.SaveRetentionPeriod(SelectedRetention);
        await Shell.Current.DisplayAlert("設定", "保存されました", "OK");
        // ✅ 前の画面に戻る（ShellルートならこれでOK）
        //await Shell.Current.GoToAsync("//CandidateCategoryPage");
        await Shell.Current.Navigation.PopAsync();
        //await Shell.Current.GoToAsync("..");
    }
    [RelayCommand]
    private async Task Cancel()
    {
        // ✅ 前の画面に戻る（ShellルートならこれでOK）
        //await Shell.Current.GoToAsync("..");
        await Shell.Current.Navigation.PopAsync();
    }
}


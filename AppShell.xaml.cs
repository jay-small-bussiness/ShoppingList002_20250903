using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ShoppingList002.Models.UiModels;
using ShoppingList002.Views;
using ShoppingList002.Services;
using ShoppingList002.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows.Input;
//using ShoppingList002.Views;

namespace ShoppingList002
{
    public partial class AppShell : Shell
    {
        //private readonly IDatabaseService _databaseService;
        //private readonly ICandidateService _candidateService;
        private readonly IServiceProvider _serviceProvider;
        public ICommand NavigateToSettingsCommand { get; }

        //public AppShell(IDatabaseService databaseService, ICandidateService candidateService, IServiceProvider serviceProvider)
        public AppShell(IServiceProvider serviceProvider)
        {
            InitializeComponent();
           
            //_databaseService = databaseService;
            //_candidateService = candidateService;
            _serviceProvider = serviceProvider;
            Routing.RegisterRoute(nameof(CandidateCategoryPage), typeof(CandidateCategoryPage));
            Routing.RegisterRoute(nameof(ShoppingListPage), typeof(ShoppingListPage));
            Routing.RegisterRoute("candidatelist", typeof(CandidateListPage));
            //Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            NavigateToSettingsCommand = new Command(async () =>
            {
                // ✅ まずFlyoutを閉じる
                Shell.Current.FlyoutIsPresented = false;
                // ✅ 設定画面を開く
                var settingsPage = _serviceProvider.GetRequiredService<SettingsPage>();
                await Shell.Current.Navigation.PushAsync(settingsPage);
            });

            BindingContext = this;
            //InitializeFlyoutItems();


        }
        public async Task InitializeFlyoutItems()
        {
            //var vm = _serviceProvider.GetService<CandidateCategoryViewModel>();
            //var page = new CandidateCategoryPage(vm, _serviceProvider);

            //var flyoutItem = new FlyoutItem
            //{
            //    Title = "カテゴリ一覧",
            //    Items =
            //    {
            //        new ShellContent
            //        {
            //            Title = "カテゴリ",
            //            ContentTemplate = new DataTemplate(() =>
            //            {
            //                var vm = _serviceProvider.GetService<CandidateCategoryViewModel>();
            //                return new CandidateCategoryPage(vm,_serviceProvider);
            //            })
            //        }
            //    }
            //};

            //Items.Insert(0, flyoutItem);
        }

    }
}
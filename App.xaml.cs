using ShoppingList002.Models.UiModels;
using ShoppingList002.Services;
using ShoppingList002.Views;
namespace ShoppingList002
{
    public partial class App : Application
{
    public IServiceProvider Services { get; }

    public App(
        IServiceProvider services,
        IInitializationService initializer,
        AppShell appShell,
        IDatabaseService databaseService)
    {
        InitializeComponent();

        Services = services;

        MainPage = new SplashPage();

        Task.Run(async () =>
        {
            Console.WriteLine("🔧 DB初期化 開始");
            await databaseService.InitializeDatabaseAsync();
            Console.WriteLine("✅ DB初期化 完了");
            await appShell.InitializeFlyoutItems();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                MainPage = appShell;
            });
        });
    }
}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingList002.Services
{
    public class InitializationService : IInitializationService
    {
        private readonly IDatabaseService _dbService;
        private readonly AppShell _appShell;

        public InitializationService(IDatabaseService dbService, AppShell appShell)
        {
            _dbService = dbService;
            _appShell = appShell;
        }

        public async Task InitializeAppAsync()
        {
            Console.WriteLine("🔧 DB初期化 開始");
            await _dbService.InitializeDatabaseAsync();
            Console.WriteLine("✅ DB初期化 完了");

            await _appShell.InitializeFlyoutItems();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage = _appShell;
            });
        }
    }

}

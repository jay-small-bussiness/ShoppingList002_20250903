using Microsoft.Extensions.Logging;
using ShoppingList002.Services;
using ShoppingList002.ViewModels;
using ShoppingList002.Views;
using CommunityToolkit.Maui;

namespace ShoppingList002
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // App にサービス注入できるように DI 替え
            builder.Services.AddSingleton<App>(sp =>
                new App(
                    services: sp,
                    initializer: sp.GetRequiredService<IInitializationService>(),
                    appShell: sp.GetRequiredService<AppShell>(),
                    databaseService: sp.GetRequiredService<IDatabaseService>())
            );
            builder.Services.AddSingleton<App>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<ICandidateService, CandidateService>();
            builder.Services.AddSingleton<IShoppingListService, ShoppingListService>();
            builder.Services.AddSingleton<IInitializationService, InitializationService>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<ISoundService, SoundService>();
            builder.Services.AddSingleton<IActivityLogService, ActivityLogService>();

#if ANDROID
            builder.Services.AddSingleton<ISpeechToTextService, ShoppingList002.Platforms.Android.SpeechToTextService>();
            builder.Services.AddSingleton<IAudioFeedbackService, ShoppingList002.Platforms.Android.AudioFeedbackService>();
#endif

            builder.Services.AddSingleton<CandidateCategoryViewModel>();
            builder.Services.AddTransient<CandidateCategoryPage>();

            builder.Services.AddTransient<CandidateListPageViewModel>();
            builder.Services.AddTransient<CandidateListPage>();

            builder.Services.AddTransient<ShoppingListPageViewModel>();
            builder.Services.AddTransient<ShoppingListPage>();

            builder.Services.AddTransient<EditCategoryPopupViewModel>();
            //builder.Services.AddTransient<EditCategoryPopupPage>(); // ← これを追加！

            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<SettingsPageViewModel>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddTransient<VoiceSearchPage>();
            builder.Services.AddTransient<VoiceSearchViewModel>();

            builder.Services.AddSingleton<ActivityLogService>();
            builder.Services.AddTransient<ActivityLogPage>();
            builder.Services.AddTransient<ActivityLogPageViewModel>();

            //return builder.Build();
            var app = builder.Build();

            // ServiceHelper にサービスプロバイダ登録
            ServiceHelper.Services = app.Services;

            return app;
        }
    }
}

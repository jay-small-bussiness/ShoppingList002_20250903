using Android.Runtime;
using ShoppingList002.Platforms.Android;
using ShoppingList002.Services;

namespace ShoppingList002.Views;

public partial class SplashPage : ContentPage
{
    private readonly IInitializationService _init;
    public SplashPage()
	{
		InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LogoLabel.Opacity = 0;
        await LogoLabel.FadeTo(1, 100);
        await Task.Delay(1000);
        await LogoLabel.FadeTo(0, 2000);

        //var csv = "豚こま,1285,1285,500,名詞,一般,*,*,*,*,豚こま,ブタコマ,ブタコマ";
        //var csv = "豚こま,1285,1285,500,名詞,一般,*,*,*,*,豚こま,ブタコマ,ブタコマ";
        //var csv = "豚こま,1285,1285,500,名詞,固有名詞,一般,*,*,*,豚こま,ブタコマ,ブタコマ";
        //var csv = "豚こま,1285,1285,500,名詞,固有名詞,一般,*,*,*,豚こま,ブタコマ,ブタコマ\r\n";
        // csvを確実にBOMなしUTF-8にする
        //var csv = "東京スカイツリー,1285,1285,500,名詞,固有名詞,一般,*,*,*,東京スカイツリー,トウキョウスカイツリー,トウキョウスカイツリー\n";
        //var csv = "豚こま,1285,1285,500,名詞,固有名詞,一般,*,*,*,豚こま,ブタコマ,ブタコマ\n";
        //var csv = "東京,1285,1285,500,名詞,固有名詞,一般,*,*,*,東京,トウキョウ,トウキョウ\n";
        //var csv = "豚こま,1285,1285,500,名詞,固有名詞,一般,*,*,*,豚コマ,ブタコマ,ブタコマ\n";

        // 1文字ずつ Unicode コードポイントを表示
        //foreach (var c in csv)
        //{
        //    System.Diagnostics.Debug.WriteLine($"{c} U+{(int)c:X4}");
        //}
        //csv = csv.TrimStart('\uFEFF');  // BOM削除
        try
        {
            //AndroidKanaBridge.SetUserDictionary(csv);
            //var path = Path.Combine(FileSystem.AppDataDirectory, "userdict.csv");
            using var stream = Android.App.Application.Context.Assets.Open("userdict.csv");
            using var reader = new StreamReader(stream);
            var csvText = reader.ReadToEnd();
            AndroidKanaBridge.SetUserDictionary(csvText);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ex={ex.Message}"); 
        }
        //AndroidKanaBridge.ToKatakana();
        var kata = AndroidKanaBridge.ToKatakana("豚こま");
        //var kata = AndroidKanaBridge.ToKatakana("東京スカイツリー");
        System.Diagnostics.Debug.WriteLine($"KATA={kata}"); // 期待: ブタコマ

        var cls = JNIEnv.FindClass("com/yourapp/kana/KanaConverter");
        Console.WriteLine($"CLASS PTR={cls}");
        var mid = JNIEnv.GetStaticMethodID(cls, "getReadingKatakana", "(Ljava/lang/String;)Ljava/lang/String;");
        Console.WriteLine($"METHOD PTR={mid}");




        // 初期化処理をここで！
        var initializer = ServiceHelper.GetService<IInitializationService>();
        await initializer.InitializeAppAsync();

        var appShell = ServiceHelper.GetService<AppShell>();
        Application.Current.MainPage = appShell;
    }
}
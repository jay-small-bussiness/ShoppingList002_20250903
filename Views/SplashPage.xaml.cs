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

        //var csv = "�؂���,1285,1285,500,����,���,*,*,*,*,�؂���,�u�^�R�},�u�^�R�}";
        //var csv = "�؂���,1285,1285,500,����,���,*,*,*,*,�؂���,�u�^�R�},�u�^�R�}";
        //var csv = "�؂���,1285,1285,500,����,�ŗL����,���,*,*,*,�؂���,�u�^�R�},�u�^�R�}";
        //var csv = "�؂���,1285,1285,500,����,�ŗL����,���,*,*,*,�؂���,�u�^�R�},�u�^�R�}\r\n";
        // csv���m����BOM�Ȃ�UTF-8�ɂ���
        //var csv = "�����X�J�C�c���[,1285,1285,500,����,�ŗL����,���,*,*,*,�����X�J�C�c���[,�g�E�L���E�X�J�C�c���[,�g�E�L���E�X�J�C�c���[\n";
        //var csv = "�؂���,1285,1285,500,����,�ŗL����,���,*,*,*,�؂���,�u�^�R�},�u�^�R�}\n";
        //var csv = "����,1285,1285,500,����,�ŗL����,���,*,*,*,����,�g�E�L���E,�g�E�L���E\n";
        //var csv = "�؂���,1285,1285,500,����,�ŗL����,���,*,*,*,�؃R�},�u�^�R�},�u�^�R�}\n";

        // 1�������� Unicode �R�[�h�|�C���g��\��
        //foreach (var c in csv)
        //{
        //    System.Diagnostics.Debug.WriteLine($"{c} U+{(int)c:X4}");
        //}
        //csv = csv.TrimStart('\uFEFF');  // BOM�폜
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
        var kata = AndroidKanaBridge.ToKatakana("�؂���");
        //var kata = AndroidKanaBridge.ToKatakana("�����X�J�C�c���[");
        System.Diagnostics.Debug.WriteLine($"KATA={kata}"); // ����: �u�^�R�}

        var cls = JNIEnv.FindClass("com/yourapp/kana/KanaConverter");
        Console.WriteLine($"CLASS PTR={cls}");
        var mid = JNIEnv.GetStaticMethodID(cls, "getReadingKatakana", "(Ljava/lang/String;)Ljava/lang/String;");
        Console.WriteLine($"METHOD PTR={mid}");




        // �����������������ŁI
        var initializer = ServiceHelper.GetService<IInitializationService>();
        await initializer.InitializeAppAsync();

        var appShell = ServiceHelper.GetService<AppShell>();
        Application.Current.MainPage = appShell;
    }
}
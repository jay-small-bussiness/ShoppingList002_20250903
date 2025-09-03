using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Application = Android.App.Application;
using ShoppingList002.Services;
using Android.Media;


namespace ShoppingList002.Platforms.Android;

public class SpeechToTextService : Java.Lang.Object, ISpeechToTextService, IRecognitionListener
{
    TaskCompletionSource<string> _tcs;
    SpeechRecognizer _speechRecognizer;

    public SpeechToTextService()
    {
        _speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Application.Context);
        _speechRecognizer.SetRecognitionListener(this);
    }

    public Task<string> RecognizeAsync()
    {
        const string recordAudioPermission = "android.permission.RECORD_AUDIO";

        if (ContextCompat.CheckSelfPermission(Application.Context, recordAudioPermission) != Permission.Granted)
        {
            var activity = Platform.CurrentActivity ?? throw new Exception("Activity not found");
            activity.RunOnUiThread(() =>
            {
                ActivityCompat.RequestPermissions(activity, new string[] { recordAudioPermission }, 10);
            });

            return Task.FromResult("マイクの許可が必要です");
        }
        // StartListening() の前に追加
        var audioManager = (AudioManager)Platform.AppContext.GetSystemService(Context.AudioService);
        var originalMode = audioManager.RingerMode;

        if (originalMode == RingerMode.Normal)
        {
            Console.WriteLine("🔕 一時的にマナーモードに切り替えます");
            audioManager.RingerMode = RingerMode.Vibrate;
        }

        _tcs = new TaskCompletionSource<string>();

        var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
        intent.PutExtra("android.speech.extra.SUPPRESS_BEEP", true);

        intent.PutExtra("android.speech.extra.DICTATION_MODE", true);
        intent.PutExtra("android.speech.extra.SUPPRESS_BEEP", true); // ← これ！
        intent.PutExtra("android.speech.extra.PROMPT", "");
        intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
        intent.PutExtra(RecognizerIntent.ExtraPrompt, "話してください");
        intent.PutExtra(RecognizerIntent.ExtraLanguage, "ja-JP");

        _speechRecognizer.StartListening(intent);
        // 音声認識終了時にモードを戻す
        Task.Delay(100); // 少し待つ（ビープ対策）
        audioManager.RingerMode = originalMode;
        Console.WriteLine("🔔 元のモードに戻しました");

        return _tcs.Task;
    }
    
    public void OnResults(Bundle results)
    {
        var matches = results?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
        string result = matches?.FirstOrDefault() ?? "";
        _tcs.TrySetResult(result);
    }

    public void OnError([GeneratedEnum] SpeechRecognizerError error)
    {
        _tcs.TrySetResult("");
    }

    // 他イベントは空でOK
    public void OnBeginningOfSpeech() { }
    public void OnBufferReceived(byte[] buffer) { }
    public void OnEndOfSpeech() { }
    public void OnEvent(int eventType, Bundle @params) { }
    public void OnPartialResults(Bundle partialResults) { }
    public void OnReadyForSpeech(Bundle @params) { }
    public void OnRmsChanged(float rmsdB) { }
}

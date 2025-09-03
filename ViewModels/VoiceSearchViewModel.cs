using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ShoppingList002.Services;
using ShoppingList002.Models.UiModels;
using ShoppingList002.Services.Converters;
using ShoppingList002.Models.DbModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using static Android.Renderscripts.ScriptGroup;
//using Microsoft.UI.Xaml.Controls.Primitives;

namespace ShoppingList002.ViewModels;

public partial class VoiceSearchViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private bool _allowAutoRestart = true;
    private CancellationTokenSource _cts;
    public Color MicStatusColor => IsListening ? Colors.Red : Colors.Gray;
    public string MicStatusText => IsListening ? "🎤 話しかけてください\n📁 カテゴリー○○で呼び出せます\n🛑 「おしまい」で終わります" : "";

    private string _recognizedText;
    private bool _isListening;
    public bool IsListening
    {
        get => _isListening;
        set
        {
            if (_isListening != value)
            {
                _isListening = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MicStatusColor));
                OnPropertyChanged(nameof(MicStatusText));
            }
        }
    }
    private bool _showExpandButton;
    public bool ShowExpandButton
    {
        get => _showExpandButton;
        set => SetProperty(ref _showExpandButton, value);
    }

    public string RecognizedText
    {
        get => _recognizedText;
        set
        {
            if (_recognizedText != value)
            {
                _recognizedText = value;
                OnPropertyChanged(); // ← これ
            }
        }
    }
    public ObservableCollection<SearchResultItemModel> SearchResults { get; set; } = new();
    public ObservableCollection<string> AddedHistory { get; } = new();

    // 🔁 ステート制御
    public enum VoiceInputState
    {
        Idle,           // 何もしていない
        Listening,      // マイク入力中
        Processing,     // 結果処理中
        Choosing,       // 複数候補からの選択待ち
        NoInput,        // 無言
        NotFound,       // 0件ヒット
        Done            // 終了
    }

    [ObservableProperty]
    private VoiceInputState currentState = VoiceInputState.Idle;
    // 追加：UI文言
    [ObservableProperty] private string modeChipText = "モード：通常検索";
    [ObservableProperty] private string promptText = "追加する品名を話してください";
    [ObservableProperty] private string hintText = "例：『トマト』『牛乳』／『カテゴリー ○○』でページへ／『おしまい』で終了";

    // 🔧 DIサービス
    private readonly ISpeechToTextService _speechService;
    private readonly ISoundService _soundService;
    private readonly ICandidateService _candidateService;
    private readonly IShoppingListService _shoppingListService;
    private readonly IActivityLogService _logService;

    public VoiceSearchViewModel(
     ISpeechToTextService speechService,
     ISoundService soundService,
     ICandidateService candidateService,
     IShoppingListService shoppingListService,
     IActivityLogService logService,
     ISettingsService settings) // ★ 追加
    {
        _speechService = speechService;
        _soundService = soundService;
        _candidateService = candidateService;
        _shoppingListService = shoppingListService;
        _logService = logService;
        _settings = settings; // ★ 追加

        // 応答形式の初期値（Preferences）
        SelectedVoiceModeIndex = _settings.LoadVoiceFeedbackMode() == VoiceFeedbackMode.BeepOnly ? 0 : 1;

        UpdateUiTexts(); // ★ 初期表示文言
    }

    // 📋 候補一覧（複数ヒット時）
    public ObservableCollection<CandidateListItemUiModel> CandidateItems { get; } = new();

    // 📝 入力文字列保持（0件時の処理で使う）
    [ObservableProperty]
    private string lastRecognizedText;
    [RelayCommand]
    private async Task RetryAsync()
    {
        // “やり直し”：結果クリア→Idle→再聴取
        SearchResults.Clear();
        RecognizedText = "";
        CurrentState = VoiceInputState.Idle;
        UpdateUiTexts();
        await Task.Delay(100);
        if (!_allowAutoRestart) return;   // ← 追加
        await StartListeningAsync();
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        // “おしまい”：終了表示→前ページへ戻る
        _allowAutoRestart = false;         // ← 自動再開を抑止
        _cts?.Cancel();                    // ← 認識を止める（対応してるなら）
        IsListening = false;

        CurrentState = VoiceInputState.Done;
        UpdateUiTexts();
        IsListening = false;
        RecognizedText = "音声入力を終了しました";
        await Task.Delay(600);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task StopListeningAsync()
    {
        _allowAutoRestart = false;         // ← 同上
        _cts?.Cancel();
        IsListening = false;
        CurrentState = VoiceInputState.Idle;
        UpdateUiTexts();
    }

    // 🎤 音声認識スタート
    [RelayCommand]
    public async Task StartListeningAsync()
    {
        _allowAutoRestart = true;              // ← 開始時は許可
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        SearchResults.Clear();

        CurrentState = VoiceInputState.Listening;
        UpdateUiTexts(); // ★ 追加
        IsListening = true;
        RecognizedText = "";
        //_soundService.Play("start");           // 登録済みメッセージ表示（UI側で監視）
        var result = await _speechService.RecognizeAsync();

        if (string.IsNullOrWhiteSpace(result))
        {
            CurrentState = VoiceInputState.NoInput;
            await HandleNull();
            return;
        }
        // ✅ カテゴリ指定のコマンド判定
        if (TryParseCategoryCommand(result, out var categoryName))
        {
            var category = await _candidateService.FindCategoryByNameAsync(categoryName);
            if (category != null)
            {
                var route = $"candidatelist?categoryId={category.CategoryId}&categoryTitle={category.Title}&CategoryTitleWithEmoji={category.IconName}&colorId={category.ColorId}&fromVoice=true";
                await Shell.Current.GoToAsync(route);
                return;
            }
            else
            {
                // 見つからなかった場合：表示だけして終わる？
                IsListening = false;
                RecognizedText = $"「{categoryName}」カテゴリは見つかりませんでした";
                await Task.Delay(2000);
                await StartListeningAsync(); // 自動再入力

                return;
            }
        }
        LastRecognizedText = result;

        CurrentState = VoiceInputState.Processing;

        RecognizedText = LastRecognizedText;
        //var matches = await _candidateService.SearchByNameAsync(result, false);
        var matches = await _candidateService.SearchByNameAsync(result);
        IsListening = false;
        if (IsEndCommand(result))  //「おしまい」を感知する
        {
            CurrentState = VoiceInputState.Done;
            IsListening = false;
            RecognizedText = "音声入力を終了しました";
            await Task.Delay(1000);
            // ✅ ShoppingListPage に戻る
            //await Shell.Current.GoToAsync("///ShoppingListPage");
            await Shell.Current.GoToAsync(".."); // ← ひとつ前のページに戻る

            return;
        }
        // 1) まずStrictキーの完全一致を探す（1件なら即AutoPick）
        var keyStrict = KanaHelper.ToSearchKana(result);
        //foreach (var r in matches)
        //{
        //    Console.WriteLine($"[DEBUG] matches: {r.ItemName}");
        //}
        var exacts = matches.Where(r => KanaHelper.ToSearchKana(r.ItemName) == keyStrict).ToList();
        if (exacts.Count() == 1)
        {
            await HandleSingleHit(exacts[0]);
            return;
        }
        // 2) 完全一致が無い場合：トップ=100 かつ 2位<100 ならAutoPick
        if (matches.Count >= 1 && matches[0].Score == 100 &&
            (matches.Count == 1 || matches[1].Score < 100))
        {
            await HandleSingleHit(matches[0]);
            return;
        }

        if (matches.Count == 0)　　//音声入力で検索結果マッチなし
        {
            CurrentState = VoiceInputState.NotFound;
        }
        else if (matches.Count == 1)
        {
            //await HandleSingleHit(matches[0]);
        }
        else
        {
            await HandleMultipleHits(matches);
            return;
        }
    }

    private async Task HandleSingleHit(SearchResultItemModel item)
    {
        bool already = await _shoppingListService.ExistsAsync(item.ItemId);

        if (already)
        {
            _soundService.Play("already");           // 登録済みメッセージ表示（UI側で監視）
            AddedHistory.Insert(0, $"{DateTime.Now:HH:mm:ss} 「{item.ItemName}」登録済！");
        }
        else
        {
            SearchResults.Clear();
            SearchResults.Add(new SearchResultItemModel
            {
                CategoryName = item.CategoryName,
                ItemId = item.ItemId,
                BackgroundColor = item.BackgroundColor,
                ItemName = item.ItemName
            });

            _soundService.Play("added");
            await _shoppingListService.AddItemAsync(item.ItemId);
            AddedHistory.Insert(0, $"{DateTime.Now:HH:mm:ss} 「{item.ItemName}」追加！");

            await _logService.LogAsync("ADD", item.ItemId, item.ItemName, "", "");

        }

        await Task.Delay(500);
        if (!_allowAutoRestart) return;   // ← 追加
        await StartListeningAsync();
    }

    private async Task HandleMultipleHits(List<SearchResultItemModel> items)
    {
        SearchResults.Clear();
        //CandidateItems.Clear();
        foreach (var item in items)
            //CandidateItems.Add(item);
            SearchResults.Add(new SearchResultItemModel
            {
                CategoryName = item.CategoryName,
                ItemId = item.ItemId,
                BackgroundColor = item.BackgroundColor,
                ItemName = item.ItemName
            });

        _soundService.Play("multiple");
        CurrentState = VoiceInputState.Choosing;
    }
    private async Task HandleNull()
    {
        SearchResults.Clear();
        CurrentState = VoiceInputState.Choosing;
        await Task.Delay(500);
        if (!_allowAutoRestart) return;   // ← 追加
        await StartListeningAsync();
    }
    private bool TryParseCategoryCommand(string input, out string categoryName)
    {
        categoryName = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim();

        if (input.StartsWith("カテゴリー") || input.StartsWith("カテゴリ"))
        {
            categoryName = input.Replace("カテゴリー", "")
                                .Replace("カテゴリ", "")
                                .Trim();
            return !string.IsNullOrEmpty(categoryName);
        }

        return false;
    }

    public async Task OnItemSelectedAsync(SearchResultItemModel item)
    {
        bool already = await _shoppingListService.ExistsAsync(item.ItemId);

        if (already)
        {
            _soundService.Play("already");           // 登録済みメッセージ表示（UI側で監視）
            AddedHistory.Insert(0, $"{DateTime.Now:HH:mm:ss} 「{item.ItemName}」登録済！");
        }
        else
        {
            await _shoppingListService.AddItemAsync(item.ItemId);
            AddedHistory.Insert(0, $"{DateTime.Now:HH:mm:ss} 「{item.ItemName}」追加！");
            _soundService.Play("added");
        }


        // ✅ トースト通知とか
        //await Shell.Current.DisplayAlert("追加完了", $"{item.ItemName} をリストに追加しました", "OK");

        // ✅ 検索結果をクリアしたいならこっちも
        SearchResults.Clear();

        await Task.Delay(500);
        if (!_allowAutoRestart) return;   // ← 追加
        await StartListeningAsync();
    }
    private bool IsEndCommand(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        text = text.Trim();
        return text.Contains("終了") || text.Contains("おしまい");
    }
    // VoiceSearchViewModel.cs の一部イメージ
    public async Task SearchAsync(string input, bool loosen = false)
    {
        //var results = await _candidateService.SearchByNameAsync(input, loosen);
        var results = await _candidateService.SearchByNameAsync(input);
        SearchResults = new ObservableCollection<SearchResultItemModel>(results);

        ShowExpandButton = !loosen && results.Count <= 2;
        OnPropertyChanged(nameof(ShowExpandButton));

        // SingleHitなら0.5秒後に自動追加（既存挙動）
        if (results.Count == 1 && !loosen)
        {
            await Task.Delay(500);
            await _shoppingListService.AddItemAsync(results[0].ItemId);
            //await AddToShoppingListAsync(results[0]);
        }
    }

    public async Task ExpandSearchAsync()
    {
        await SearchAsync(LastRecognizedText, loosen: true);
        ShowExpandButton = false;
    }

    public async Task HandleSelection(CandidateListItemUiModel selected)
    {
        bool already = await _shoppingListService.ExistsAsync(selected.ItemId);

        if (already)
        {
            _soundService.Play("already");
        }
        else
        {
            _soundService.Play("added");
            await _shoppingListService.AddItemAsync(selected.ItemId);
            await _logService.LogAsync("ADD", selected.ItemId, selected.Name, "", "");
        }

        await Task.Delay(500);
        if (!_allowAutoRestart) return;   // ← 追加
        await StartListeningAsync();
    }
    public List<string> VoiceModeOptions { get; } = new() { "ピンポン音のみ", "説明型" };
    private int _selectedVoiceModeIndex;
    public int SelectedVoiceModeIndex
    {
        get => _selectedVoiceModeIndex;
        set
        {
            if (_selectedVoiceModeIndex != value)
            {
                _selectedVoiceModeIndex = value;
                OnPropertyChanged();
                var mode = value == 0 ? VoiceFeedbackMode.BeepOnly : VoiceFeedbackMode.ExplainTTS;
                _settings.SaveVoiceFeedbackMode(mode);
            }
        }
    }
    private void UpdateUiTexts()
    {
        switch (CurrentState)
        {
            case VoiceInputState.Idle:
                ModeChipText = "モード：通常検索";
                PromptText = "追加する品名を話してください";
                HintText = "例：『トマト』『牛乳』／『カテゴリー ○○』でページへ／『おしまい』で終了";
                break;

            case VoiceInputState.Listening:
                ModeChipText = "モード：通常検索（聴取中）";
                PromptText = "お話しください";
                HintText = "『おしまい』で終了します";
                break;

            case VoiceInputState.Choosing:
                ModeChipText = "モード：候補から選択";
                PromptText = "候補をタップしてください";
                HintText = "『おしまい』で終了します";
                break;

            case VoiceInputState.NotFound:
                ModeChipText = "モード：0件";
                PromptText = "見つかりませんでした。もう一度どうぞ";
                HintText = "言い換え例：『トマト』→『とまと』";
                break;

            case VoiceInputState.Done:
                ModeChipText = "モード：終了";
                PromptText = "音声入力を終了しました";
                HintText = "また使うときは🎤を押してください";
                break;

            default:
                break;
        }
    }
    private async Task SpeakOrBeepAsync(string textKanaOrPlain)
    {
        var mode = _settings.LoadVoiceFeedbackMode();
        if (mode == VoiceFeedbackMode.BeepOnly)
        {
            _soundService.Play("notify"); // 既存のadded/multiple/alreadyでも可
        }
        else
        {
            // ★ TTSサービスをまだVMにDIしてないなら後回しでもOK。
            // まずはビープにフォールバックしておく。
            try
            {
                // await _ttsService.SpeakAsync(textKanaOrPlain); // あとで差し替え
                _soundService.Play("notify");
            }
            catch
            {
                _soundService.Play("notify");
            }
        }
    }
    public enum VoiceInputMode { GlobalSearch, AddToSpecificCategory }

    [ObservableProperty] private VoiceInputMode currentMode = VoiceInputMode.GlobalSearch;
    private int? _fixedCategoryId = null;

    // カテゴリページから呼ぶ（遷移時に）
    public void StartAddToSpecificCategory(int categoryId)
    {
        _fixedCategoryId = categoryId;
        CurrentMode = VoiceInputMode.AddToSpecificCategory;
        CurrentState = VoiceInputState.Idle;
        ModeChipText = $"モード：カテゴリ連続追加（ID={categoryId}）";
        PromptText = "追加するアイテムを話してください";
        HintText = "例：『あらびき黒こしょう』『ねりからし』／『おしまい』で終了";
    }
}

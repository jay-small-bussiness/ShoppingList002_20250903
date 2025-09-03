using Microsoft.Maui.Storage;
using ShoppingList002.Models.UiModels;
namespace ShoppingList002.Services;

public class SettingsService : ISettingsService
{
    private const string RetentionKey = "DeletedDataRetention";
    // 追加：応答形式のキーとデフォルト
    private const string VoiceFeedbackModeKey = "VoiceFeedbackMode";
    private const VoiceFeedbackMode DefaultVoiceMode = VoiceFeedbackMode.BeepOnly;

    public void SaveRetentionPeriod(string value)
    {
        Preferences.Set(RetentionKey, value);
    }

    public string LoadRetentionPeriod()
    {
        return Preferences.Get(RetentionKey, "1ヶ月");
    }
    public int GetRetentionDays()
    {
        var value = LoadRetentionPeriod();

        return value switch
        {
            "1ヶ月" => 30,
            "半年" => 180,
            "1年" => 365,
            "5年" => 365 * 5,
            _ => 30
        };
    }
    // ==== ここから追加 ====
    public void SaveVoiceFeedbackMode(VoiceFeedbackMode mode)
    {
        Preferences.Set(VoiceFeedbackModeKey, (int)mode);
    }

    public VoiceFeedbackMode LoadVoiceFeedbackMode()
    {
        var v = Preferences.Get(VoiceFeedbackModeKey, (int)DefaultVoiceMode);
        return Enum.IsDefined(typeof(VoiceFeedbackMode), v)
            ? (VoiceFeedbackMode)v
            : DefaultVoiceMode;
    }

}

using ShoppingList002.Models.UiModels;

namespace ShoppingList002.Services
{
    public interface ISettingsService
    {
        void SaveRetentionPeriod(string value);
        string LoadRetentionPeriod();
        int GetRetentionDays();
        void SaveVoiceFeedbackMode(VoiceFeedbackMode mode);
        VoiceFeedbackMode LoadVoiceFeedbackMode();
    }
}

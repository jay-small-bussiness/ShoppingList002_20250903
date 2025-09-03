using SQLite;
using ShoppingList002.Models.DbModels;

namespace ShoppingList002.Models.UiModels
{
    public enum VoiceFeedbackMode
    {
        BeepOnly = 0,
        ExplainTTS = 1
    }

    public class AppSettingsService
    {
        private readonly SQLiteAsyncConnection _conn;
        private const string Key_VoiceFeedbackMode = "VoiceFeedbackMode";
        private const VoiceFeedbackMode DefaultMode = VoiceFeedbackMode.BeepOnly;

        public AppSettingsService(SQLiteAsyncConnection conn)
        {
            _conn = conn;
        }

        public async Task<VoiceFeedbackMode> GetVoiceFeedbackModeAsync()
        {
            var record = await _conn.Table<AppSettingDbModel>()
                .Where(x => x.Key == Key_VoiceFeedbackMode)
                .FirstOrDefaultAsync();

            if (record == null)
            {
                // 無ければデフォ値を保存
                await SetVoiceFeedbackModeAsync(DefaultMode);
                return DefaultMode;
            }

            if (int.TryParse(record.Value, out var intValue) &&
                Enum.IsDefined(typeof(VoiceFeedbackMode), intValue))
            {
                return (VoiceFeedbackMode)intValue;
            }

            // 値が壊れてたらデフォ値に戻す
            await SetVoiceFeedbackModeAsync(DefaultMode);
            return DefaultMode;
        }

        public async Task SetVoiceFeedbackModeAsync(VoiceFeedbackMode mode)
        {
            var record = await _conn.Table<AppSettingDbModel>()
                .Where(x => x.Key == Key_VoiceFeedbackMode)
                .FirstOrDefaultAsync();

            if (record == null)
            {
                record = new AppSettingDbModel
                {
                    Key = Key_VoiceFeedbackMode,
                    Value = ((int)mode).ToString(),
                    UpdatedDate = DateTimeOffset.Now
                };
                await _conn.InsertAsync(record);
            }
            else
            {
                record.Value = ((int)mode).ToString();
                record.UpdatedDate = DateTimeOffset.Now;
                await _conn.UpdateAsync(record);
            }
        }
    }


}

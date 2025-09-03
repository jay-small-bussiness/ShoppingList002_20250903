using Android.Media;
using Android.Content.Res;
using Microsoft.Maui.ApplicationModel;
using ShoppingList002.Services;

namespace ShoppingList002.Platforms.Android
{
    public class AudioFeedbackService : IAudioFeedbackService
    {
        private readonly Dictionary<FeedbackType, MediaPlayer> _players = new();
        private readonly Dictionary<FeedbackType, string> _soundMap = new()
        {
            { FeedbackType.Start, "start" },
            { FeedbackType.OneHitAdded, "added" },
            { FeedbackType.MultiHit, "multiple" },
            { FeedbackType.NoHit, "nohit" },
            { FeedbackType.AlreadyExists, "already" }
        };
        public AudioFeedbackService()
        {
            // 効果音ファイル名と対応
            var soundMap = new Dictionary<FeedbackType, string>
            {
                { FeedbackType.Start, "start" },
                { FeedbackType.OneHitAdded, "added" },
                { FeedbackType.MultiHit, "multiple" },
                { FeedbackType.NoHit, "nohit" },
                { FeedbackType.AlreadyExists, "already" }
            };

            foreach (var kv in soundMap)
            {
                var name = kv.Value;
                var id = Platform.CurrentActivity.Resources.GetIdentifier(name, "raw", Platform.AppContext.PackageName);

                Console.WriteLine($"🎧 読み込み試行: {name}, ID = {id}");

                if (id == 0)
                {
                    Console.WriteLine($"❌ Resource not found: {name}.mp3");
                    continue; // これで例外回避して残りを試せる
                }

                var player = MediaPlayer.Create(Platform.AppContext, id);
                _players[kv.Key] = player;
            }
        }

        public void PlaySound(FeedbackType type)
        {
            if (!_players.ContainsKey(type))
            {
                var resourceId = Platform.CurrentActivity.Resources.GetIdentifier(
                    _soundMap[type], "raw", Platform.AppContext.PackageName);

                if (resourceId != 0)
                {
                    var player = MediaPlayer.Create(Platform.AppContext, resourceId);
                    _players[type] = player;
                }
                else
                {
                    Console.WriteLine($"❌ Resource not found: {_soundMap[type]}");
                    return;
                }
            }

            var current = _players[type];

            if (current.IsPlaying)
            {
                current.Stop();
                current.Prepare();
            }

            current.Start();
        }

    }
}

using Microsoft.Maui.Controls;
using System.Reflection;
#if ANDROID
using Android.Media;
#endif
#if ANDROID
using Android.Content.Res;
using Android.App;
#endif

namespace ShoppingList002.Services
{
    public class SoundService : ISoundService
    {
        public async void Play(string soundName)
        {
#if ANDROID
            try
            {
                var fileName = $"Resources/Raw/{soundName}.mp3";
                using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);

                var tempPath = Path.Combine(FileSystem.CacheDirectory, $"{soundName}.mp3");

                using (var output = File.Create(tempPath))
                {
                    await stream.CopyToAsync(output);
                }

                var player = new MediaPlayer();
                player.SetDataSource(tempPath);
                player.Prepare();
                player.Start();

                player.Completion += (s, e) =>
                {
                    player.Release();
                    player.Dispose();
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SoundService] 効果音再生失敗: {ex.Message}");
            }
#endif
        }
    }
}



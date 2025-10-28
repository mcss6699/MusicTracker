using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace MusicTracker
{
    class Program
    {
        private const string OutputFile = "current_track.txt";
        private static string _lastTrack = "";
        private static int _errorCount = 0;
        private static readonly int _maxErrorCount = 5;
        private static readonly int _baseDelayMs = 1000; // 基础延迟1秒
        private static readonly int _errorDelayMs = 5000; // 错误后延迟5秒

        static async Task Main(string[] args)
        {
            Console.WriteLine("开始监控音乐播放...");
            Console.WriteLine($"当前播放的歌曲信息将写入: {OutputFile}");

            while (true)
            {
                string currentTrack = "未在播放";

                try
                {
                    currentTrack = await GetMediaInfoAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"获取媒体信息时发生异常: {ex.Message}"); // 可选
                    currentTrack = "未在播放";
                    _errorCount++;
                }

                if (currentTrack != "未在播放" || _errorCount == 0)
                {
                    _errorCount = 0;
                }

                if (currentTrack != _lastTrack)
                {
                    Console.WriteLine($"当前播放: {currentTrack}");
                    WriteToFile(currentTrack, OutputFile);
                    _lastTrack = currentTrack;
                }

                int delayMs = _errorCount >= _maxErrorCount ? _errorDelayMs : _baseDelayMs;
                await Task.Delay(delayMs);
            }
        }

        static async Task<string> GetMediaInfoAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                if (sessionManager == null) return "未在播放";

                var currentSession = sessionManager.GetCurrentSession();
                if (currentSession == null) return "未在播放";

                var mediaProperties = await currentSession.TryGetMediaPropertiesAsync();
                if (mediaProperties == null) return "未在播放";

                string title = !string.IsNullOrEmpty(mediaProperties.Title) ? mediaProperties.Title : "";
                string artist = !string.IsNullOrEmpty(mediaProperties.Artist) ? mediaProperties.Artist : "";

                if (string.IsNullOrEmpty(artist))
                {
                    artist = !string.IsNullOrEmpty(mediaProperties.AlbumArtist) ? mediaProperties.AlbumArtist : "";
                }
                if (string.IsNullOrEmpty(artist))
                {
                    artist = !string.IsNullOrEmpty(mediaProperties.Subtitle) ? mediaProperties.Subtitle : "";
                }

                var playbackInfo = currentSession.GetPlaybackInfo();
                bool isPlaying = playbackInfo?.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

                if (isPlaying)
                {
                    if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(artist))
                    {
                        return $"{title} - {artist}";
                    }
                    else if (!string.IsNullOrEmpty(title))
                    {
                        return title;
                    }
                    else if (!string.IsNullOrEmpty(artist))
                    {
                         return artist;
                    }
                }

                return "未在播放";
            }
            catch (Exception ex) when (!(ex is TaskCanceledException))
            {
                Console.WriteLine($"获取媒体信息时出错: {ex.Message}"); // 可选
                throw;
            }
        }

        static void WriteToFile(string content, string filename)
        {
            try
            {
                string output;
                if (content == "未在播放")
                {
                    output = "当前播放：未在播放或脚本错误"; // 未播放时输出
                }
                else
                {
                    output = $"当前播放：{content}"; // 播放时输出
                }
                File.WriteAllText(filename, output, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入文件时出错: {ex.Message}");
            }
        }
    }
}
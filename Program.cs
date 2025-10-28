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
        // 添加重置计数器和阈值
        private static int _checkCount = 0;
        private static readonly int _resetThreshold = 1800; // 重置时间
        private static GlobalSystemMediaTransportControlsSessionManager _sessionManager = null;

        static async Task Main(string[] args)
        {
            Console.WriteLine("开始监控音乐播放...");
            Console.WriteLine($"当前播放的歌曲信息将写入: {OutputFile}");

            // 初始获取 SessionManager
            await RefreshSessionManagerAsync();

            while (true)
            {
                string currentTrack = "未在播放";

                try
                {
                    // 检查是否需要重置 SessionManager
                    _checkCount++;
                    if (_checkCount >= _resetThreshold)
                    {
                        Console.WriteLine("达到重置阈值，正在刷新 SessionManager...");
                        await RefreshSessionManagerAsync();
                        _checkCount = 0; // 重置计数器
                    }

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

        // 新增：刷新 SessionManager 的方法
        static async Task RefreshSessionManagerAsync()
        {
            try
            {
                // 释放旧的引用（如果存在）
                _sessionManager = null;
                // 请求新的 SessionManager
                _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                Console.WriteLine("SessionManager 刷新成功。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刷新 SessionManager 时发生异常: {ex.Message}");
                // 如果刷新失败，保留旧的引用或置为 null，下次循环再尝试
                _sessionManager = null;
            }
        }

        static async Task<string> GetMediaInfoAsync()
        {
            // 确保 SessionManager 存在
            if (_sessionManager == null)
            {
                return "未在播放";
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            try
            {
                var currentSession = _sessionManager.GetCurrentSession();
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
                throw; // 重新抛出，让 Main 方法处理
            }
        }

        static void WriteToFile(string content, string filename)
        {
            try
            {
                string output;
                if (content == "未在播放")
                {
                    output = "当前播放：未在播放"; // 未播放时输出
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
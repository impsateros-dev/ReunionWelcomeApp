using System;
using System.IO;
using System.Media;
using System.Windows.Media;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Threading;

namespace ReunionWelcomeApp.Services;

public static class AudioService
{
    private static double _mainVolume = 0.8;
    private static double _ambientVolume = 0.3;
    private static IWavePlayer? _ambientPlayer;
    private static LoopStream? _ambientLoop;
    private static bool _isAmbientPlaying;
    private static MediaPlayer? _ambientMediaPlayer;
    private static readonly object _lock = new();

    public static void SetVolume(double volume)
    {
        _mainVolume = Math.Clamp(volume, 0, 1);
    }

    public static void SetAmbientVolume(double volume)
    {
        _ambientVolume = Math.Clamp(volume, 0, 0.5);
    }

    public static void StartAmbient(string? ambientPath = null)
    {
        try
        {
            var path = ambientPath ?? ConfigService.GetConfig().Sounds.AmbientMusic;
            if (string.IsNullOrEmpty(path)) return;

            StopAmbient();

            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            if (!File.Exists(fullPath))
            {
                LoggingService.LogWarning($"Ambient file not found: {fullPath}");
                return;
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    _ambientMediaPlayer = new MediaPlayer();
                    _ambientMediaPlayer.Open(new Uri(fullPath));
                    _ambientMediaPlayer.Volume = _ambientVolume;
                    _ambientMediaPlayer.MediaEnded += (s, e) =>
                    {
                        _ambientMediaPlayer.Position = TimeSpan.Zero;
                        _ambientMediaPlayer.Play();
                    };
                    _ambientMediaPlayer.Play();
                    _isAmbientPlaying = true;
                    LoggingService.Log($"Ambient music started: {path}");
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Failed to start ambient with MediaPlayer", ex);
                }
            });
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Failed to start ambient music", ex);
        }
    }

    public static void StopAmbient()
    {
        try
        {
            _isAmbientPlaying = false;
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                _ambientMediaPlayer?.Close();
                _ambientMediaPlayer = null;
            });
            _ambientPlayer?.Stop();
            _ambientPlayer?.Dispose();
            _ambientPlayer = null;
            _ambientLoop?.Dispose();
            _ambientLoop = null;
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Error stopping ambient", ex);
        }
    }

    public static bool IsAmbientPlaying => _isAmbientPlaying;

    public static void PlaySound(string soundPath)
    {
        lock (_lock)
        {
            try
            {
                if (string.IsNullOrEmpty(soundPath)) return;

                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, soundPath);
                if (!File.Exists(fullPath))
                {
                    LoggingService.LogWarning($"Sound file not found: {fullPath}");
                    return;
                }

                var ext = Path.GetExtension(fullPath).ToLowerInvariant();

                if (ext == ".mp3")
                {
                    PlayMp3Wpf(fullPath);
                }
                else
                {
                    PlayWavSystem(fullPath);
                }

                LoggingService.Log($"Playing sound: {soundPath}");
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to play sound: {soundPath}", ex);
            }
        }
    }

    private static void PlayWavSystem(string path)
    {
        new Thread(() =>
        {
            try
            {
                LoggingService.Log($"Playing WAV: {path}");
                using var player = new SoundPlayer(path);
                player.Play();
                Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"WAV playback failed: {path}", ex);
                PlayWavWpf(path);
            }
        }).Start();
    }

    private static void PlayWavWpf(string path)
    {
        try
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var player = new MediaPlayer();
                player.Open(new Uri(path));
                player.Volume = _mainVolume;
                player.Play();
                CleanupPlayer(player);
            });
        }
        catch (Exception ex)
        {
            LoggingService.LogError($"WPF MediaPlayer failed for: {path}", ex);
        }
    }

    private static void PlayMp3Wpf(string path)
    {
        try
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var player = new MediaPlayer();
                player.Open(new Uri(path));
                player.Volume = _mainVolume;
                player.Play();
                CleanupPlayer(player);
            });
            LoggingService.Log($"Playing MP3 via WPF: {path}");
        }
        catch (Exception ex)
        {
            LoggingService.LogError($"MP3 playback failed: {path}", ex);
        }
    }

    private static void CleanupPlayer(MediaPlayer player)
    {
        System.Threading.Tasks.Task.Delay(5000).ContinueWith(_ =>
        {
            try
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    player.Stop();
                    player.Close();
                });
            }
            catch { }
        });
    }

    public static void ForceCleanup()
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();
        LoggingService.Log("Audio memory cleanup performed");
    }

    public static void PlayStartInteraction()
    {
        var config = ConfigService.GetConfig();
        PlaySound(config.Sounds.StartInteraction);
    }

    public static void PlayTypingFeedback()
    {
        var config = ConfigService.GetConfig();
        PlaySound(config.Sounds.TypingFeedback);
    }

    public static void PlaySubmissionConfirm()
    {
        var config = ConfigService.GetConfig();
        PlaySound(config.Sounds.SubmissionConfirm);
    }

    public static void PlayNameAppear()
    {
        var config = ConfigService.GetConfig();
        if (!string.IsNullOrEmpty(config.Sounds.NameAppear))
            PlaySound(config.Sounds.NameAppear);
    }

    public static void PlayNameHighlight()
    {
        var config = ConfigService.GetConfig();
        if (!string.IsNullOrEmpty(config.Sounds.NameHighlight))
            PlaySound(config.Sounds.NameHighlight);
    }
}

public class LoopStream : WaveStream
{
    private readonly WaveStream _sourceStream;
    private VolumeSampleProvider? _volumeProvider;

    public LoopStream(WaveStream sourceStream)
    {
        _sourceStream = sourceStream;
        if (sourceStream is WaveStream waveSource)
        {
            var sampleChannel = new SampleChannel(waveSource);
            _volumeProvider = new VolumeSampleProvider(sampleChannel);
        }
    }

    public override WaveFormat WaveFormat => _sourceStream.WaveFormat;

    public override long Length => _sourceStream.Length;

    public override long Position
    {
        get => _sourceStream.Position;
        set => _sourceStream.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalBytesRead = 0;

        while (totalBytesRead < count)
        {
            int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
            if (bytesRead == 0)
            {
                if (_sourceStream.Position == 0)
                    break;
                _sourceStream.Position = 0;
            }
            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }

    public float Volume
    {
        get => _volumeProvider?.Volume ?? 1f;
        set
        {
            if (_volumeProvider != null)
                _volumeProvider.Volume = value;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _sourceStream.Dispose();
        base.Dispose(disposing);
    }
}
using System;
using System.IO;
using System.Media;
using System.Threading;

namespace ReunionWelcomeApp.Services;

public static class AudioService
{
    private static double _volume = 0.8;
    private static SoundPlayer? _player;

    public static void SetVolume(double volume) => _volume = Math.Clamp(volume, 0, 1);

    public static void PlaySound(string soundPath)
    {
        try
        {
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, soundPath);
            if (!File.Exists(fullPath))
            {
                LoggingService.LogWarning($"Sound file not found: {fullPath}");
                return;
            }
            _player = new SoundPlayer(fullPath);
            _player.Play();
            LoggingService.Log($"Playing sound: {soundPath}");
        }
        catch (Exception ex)
        {
            LoggingService.LogError($"Failed to play sound: {soundPath}", ex);
        }
    }

    public static void PlayStartInteraction() => PlaySound(ConfigService.GetConfig().Sounds.StartInteraction);
    public static void PlayTypingFeedback() => PlaySound(ConfigService.GetConfig().Sounds.TypingFeedback);
    public static void PlaySubmissionConfirm() => PlaySound(ConfigService.GetConfig().Sounds.SubmissionConfirm);
}
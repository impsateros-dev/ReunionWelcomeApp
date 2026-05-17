using System;
using System.IO;
using System.Text.Json;
using ReunionWelcomeApp.Models;

namespace ReunionWelcomeApp.Services;

public static class ConfigService
{
    private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.json");
    private static AppConfig? _config;

    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            else
            {
                _config = new AppConfig();
                Save(_config);
            }
            LoggingService.Log("Config loaded successfully");
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Failed to load config", ex);
            _config = new AppConfig();
        }
        return _config;
    }

    public static void Save(AppConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Failed to save config", ex);
        }
    }

    public static AppConfig GetConfig() => _config ?? Load();
}
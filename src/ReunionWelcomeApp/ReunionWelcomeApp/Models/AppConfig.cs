using System;

namespace ReunionWelcomeApp.Models;

public class AppConfig
{
    public SoundConfig Sounds { get; set; } = new();
    public ImageConfig Images { get; set; } = new();
    public AnimationConfig Animations { get; set; } = new();
    public NameCloudConfig NameCloud { get; set; } = new();
    public DemoConfig Demo { get; set; } = new();
}

public class SoundConfig
{
    public string StartInteraction { get; set; } = "Resources/Sounds/start.wav";
    public string TypingFeedback { get; set; } = "Resources/Sounds/typing.wav";
    public string SubmissionConfirm { get; set; } = "Resources/Sounds/confirm.wav";
    public string AmbientMusic { get; set; } = "Resources/Sounds/ambient.mp3";
    public string NameAppear { get; set; } = "Resources/Sounds/nameappear.mp3";
    public string NameHighlight { get; set; } = "Resources/Sounds/highlight.mp3";
    public double Volume { get; set; } = 0.8;
    public double AmbientVolume { get; set; } = 0.3;
}

public class ImageConfig
{
    public string Background { get; set; } = "Resources/Images/background.jpg";
    public string LogoGreen { get; set; } = "Resources/Images/Impsat ThinkAhead1.jpg";
    public string LogoBlue { get; set; } = "Resources/Images/Impsat-FondoBlanco.jpg";
}

public class AnimationConfig
{
    public int NameDisplayDuration { get; set; } = 5000;
    public int NameFadeInDuration { get; set; } = 500;
    public int NameFadeOutDuration { get; set; } = 800;
}

public class NameCloudConfig
{
    public int MaxNamesVisible { get; set; } = 50;
    public int SpecialHighlightEvery { get; set; } = 10;
    public int ExclusionRadiusPercent { get; set; } = 25;
    public string NameColor { get; set; } = "#1A237E";
    public string NameStrokeColor { get; set; } = "#FFFFFF";
    public double NameStrokeWidth { get; set; } = 2;
    public int NameMinSize { get; set; } = 18;
    public int NameMaxSize { get; set; } = 36;
    public string CounterColor { get; set; } = "#003399";
}

public class DemoConfig
{
    public bool Enabled { get; set; } = false;
    public int MockNamesCount { get; set; } = 100;
    public int IntervalMs { get; set; } = 3000;
}
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
    public string StartInteraction { get; set; } = "sounds/start.wav";
    public string TypingFeedback { get; set; } = "sounds/typing.wav";
    public string SubmissionConfirm { get; set; } = "sounds/confirm.wav";
    public double Volume { get; set; } = 0.8;
}

public class ImageConfig
{
    public string Background { get; set; } = "images/background.jpg";
    public string LogoGreen { get; set; } = "images/Impsat ThinkAhead1.jpg";
    public string LogoBlue { get; set; } = "images/Impsat-FondoBlanco.jpg";
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
}

public class DemoConfig
{
    public bool Enabled { get; set; } = false;
    public int MockNamesCount { get; set; } = 100;
    public int IntervalMs { get; set; } = 3000;
}
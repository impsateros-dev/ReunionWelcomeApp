using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Threading;
using ReunionWelcomeApp.Models;
using ReunionWelcomeApp.Services;

namespace ReunionWelcomeApp.ViewModels;

public class NameItem : INotifyPropertyChanged
{
    private double _x, _y, _size, _strokeWidth;
    private string _name = string.Empty;
    private bool _isHighlight;
    private SolidColorBrush _colorBrush = new(Colors.Black);
    private SolidColorBrush _strokeBrush = new(Colors.White);

    public double X { get => _x; set { _x = value; OnPropertyChanged(); } }
    public double Y { get => _y; set { _y = value; OnPropertyChanged(); } }
    public double Size { get => _size; set { _size = value; OnPropertyChanged(); } }
    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
    public bool IsHighlight { get => _isHighlight; set { _isHighlight = value; OnPropertyChanged(); } }
    public SolidColorBrush ColorBrush { get => _colorBrush; set { _colorBrush = value; OnPropertyChanged(); } }
    public SolidColorBrush StrokeBrush { get => _strokeBrush; set { _strokeBrush = value; OnPropertyChanged(); } }
    public double StrokeWidth { get => _strokeWidth; set { _strokeWidth = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class PresentationViewModel : INotifyPropertyChanged
{
    private string _currentName = string.Empty;
    private double _currentNameOpacity;
    private bool _showCurrentName;
    private int _attendeeCount;
    private bool _isDebugVisible;
    private double _windowWidth = 1920;
    private double _windowHeight = 1080;
    private SolidColorBrush _counterColorBrush = new(System.Windows.Media.Colors.Black);
    private readonly Random _random = new();
    private readonly List<Attendee> _allAttendees = new();
    private readonly DispatcherTimer _displayTimer;
    private readonly DispatcherTimer _fadeTimer;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<NameItem>? NameAdded;
    public event Action<NameItem>? NameRemoved;
    public event Action? NamesRepositionRequested;

    public SolidColorBrush CounterColorBrush
    {
        get => _counterColorBrush;
        set { _counterColorBrush = value; OnPropertyChanged(); }
    }

    public ObservableCollection<NameItem> NameCloud { get; } = new();

    public string CurrentName
    {
        get => _currentName;
        set { _currentName = value; OnPropertyChanged(); }
    }

    public double CurrentNameOpacity
    {
        get => _currentNameOpacity;
        set { _currentNameOpacity = value; OnPropertyChanged(); }
    }

    public bool ShowCurrentName
    {
        get => _showCurrentName;
        set { _showCurrentName = value; OnPropertyChanged(); }
    }

    public int AttendeeCount
    {
        get => _attendeeCount;
        set { _attendeeCount = value; OnPropertyChanged(); }
    }

    public bool IsDebugVisible
    {
        get => _isDebugVisible;
        set { _isDebugVisible = value; OnPropertyChanged(); }
    }

    public PresentationViewModel()
    {
        var config = ConfigService.GetConfig();

        var counterColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(config.NameCloud.CounterColor);
        _counterColorBrush = new SolidColorBrush(counterColor);

        _displayTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(config.Animations.NameDisplayDuration)
        };
        _displayTimer.Tick += (s, e) =>
        {
            _displayTimer.Stop();
            FadeOutCurrentName();
        };

        _fadeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
    }

    public void AddName(string name)
    {
        var attendee = new Attendee { FullName = name };
        _allAttendees.Add(attendee);
        AttendeeCount = _allAttendees.Count;

        System.Threading.Tasks.Task.Run(() => ExcelExportService.SaveAttendees(_allAttendees));
        LoggingService.Log($"Name added to cloud: {name}");

        ShowNewName(name);
        AddToCloud(name);

        var config = ConfigService.GetConfig();
        if (AttendeeCount % config.NameCloud.SpecialHighlightEvery == 0)
            TriggerHighlight();
    }

    private void ShowNewName(string name)
    {
        ShowCurrentName = true;
        CurrentName = name;
        CurrentNameOpacity = 1;

        var config = ConfigService.GetConfig();
        _displayTimer.Interval = TimeSpan.FromMilliseconds(config.Animations.NameDisplayDuration);
        _displayTimer.Start();
    }

    private void FadeOutCurrentName()
    {
        var config = ConfigService.GetConfig();
        var steps = 10;
        var interval = config.Animations.NameFadeOutDuration / steps;

        _fadeTimer.Interval = TimeSpan.FromMilliseconds(interval);
        var step = 0;
        _fadeTimer.Tick += (s, e) =>
        {
            step++;
            CurrentNameOpacity = 1 - (step / (double)steps);
            if (step >= steps)
            {
                _fadeTimer.Stop();
                ShowCurrentName = false;
                CurrentNameOpacity = 0;
            }
        };
        _fadeTimer.Start();
    }

    public void SetWindowSize(double width, double height)
    {
        _windowWidth = width;
        _windowHeight = height;
        LoggingService.Log($"Window size set: {width}x{height}");
    }

    public void RepositionNames(double newWidth, double newHeight)
    {
        if (newWidth <= 0 || newHeight <= 0) return;
        
        var oldWidth = _windowWidth;
        var oldHeight = _windowHeight;
        _windowWidth = newWidth;
        _windowHeight = newHeight;

        foreach (var item in NameCloud)
        {
            var relX = item.X / oldWidth;
            var relY = item.Y / oldHeight;
            item.X = Math.Max(20, Math.Min(newWidth - 100, relX * newWidth));
            item.Y = Math.Max(20, Math.Min(newHeight - 50, relY * newHeight));
        }
        
        NamesRepositionRequested?.Invoke();
        LoggingService.Log($"Repositioned {NameCloud.Count} names to {newWidth}x{newHeight}");
    }

    private static readonly System.Windows.Media.Color[] NameColors = new[]
{
    System.Windows.Media.Color.FromRgb(0, 51, 153),    // Dark blue
    System.Windows.Media.Color.FromRgb(153, 0, 51),    // Dark red
    System.Windows.Media.Color.FromRgb(0, 102, 51),    // Dark green
    System.Windows.Media.Color.FromRgb(102, 51, 0),    // Brown
    System.Windows.Media.Color.FromRgb(51, 51, 153),   // Indigo
    System.Windows.Media.Color.FromRgb(153, 51, 0),    // Orange-brown
    System.Windows.Media.Color.FromRgb(0, 77, 77),     // Teal
    System.Windows.Media.Color.FromRgb(102, 0, 102),   // Purple
    System.Windows.Media.Color.FromRgb(0, 51, 102),    // Navy
    System.Windows.Media.Color.FromRgb(128, 0, 0),     // Maroon
};

    private bool CheckOverlap(double x, double y, double size, double minDistance = 60)
    {
        foreach (var existing in NameCloud)
        {
            var dx = x - existing.X;
            var dy = y - existing.Y;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < minDistance)
                return true;
        }
        return false;
    }

    private void ReduceAllSizes(double factor = 0.85)
    {
        var config = ConfigService.GetConfig();
        foreach (var item in NameCloud)
        {
            var newSize = Math.Max(config.NameCloud.NameMinSize, item.Size * factor);
            item.Size = newSize;
        }
        NamesRepositionRequested?.Invoke();
    }

    private void AddToCloud(string name)
    {
        var config = ConfigService.GetConfig();

        var paddingX = _windowWidth * 0.10f;
        var paddingY = _windowHeight * 0.15f;
        var usableWidth = _windowWidth - paddingX * 2;
        var usableHeight = _windowHeight - paddingY * 2;

        var centerX = _windowWidth / 2;
        var centerY = _windowHeight / 2;
        var exclusionRadius = _windowWidth * (config.NameCloud.ExclusionRadiusPercent / 100.0);

        double x, y;
        int attempts = 0;
        int maxAttempts = 100;
        bool placed = false;

        do
        {
            x = paddingX + _random.NextDouble() * usableWidth;
            y = paddingY + _random.NextDouble() * usableHeight;

            var distFromCenter = Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
            if (distFromCenter >= exclusionRadius && !CheckOverlap(x, y, 24))
            {
                placed = true;
                break;
            }
            attempts++;
        } while (attempts < maxAttempts);

        if (!placed && NameCloud.Count > 0)
        {
            ReduceAllSizes(0.85);
            attempts = 0;
            do
            {
                x = paddingX + _random.NextDouble() * usableWidth;
                y = paddingY + _random.NextDouble() * usableHeight;

                var distFromCenter = Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                if (distFromCenter >= exclusionRadius && !CheckOverlap(x, y, 20))
                {
                    placed = true;
                    break;
                }
                attempts++;
            } while (attempts < maxAttempts);
        }

        if (!placed)
        {
            x = paddingX + _random.NextDouble() * usableWidth;
            y = paddingY + _random.NextDouble() * usableHeight;
        }

        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(config.NameCloud.NameColor);
        var strokeColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(config.NameCloud.NameStrokeColor);

        var item = new NameItem
        {
            Name = name,
            X = x,
            Y = y,
            Size = config.NameCloud.NameMinSize + _random.NextDouble() * (config.NameCloud.NameMaxSize - config.NameCloud.NameMinSize),
            ColorBrush = new SolidColorBrush(color),
            StrokeBrush = new SolidColorBrush(strokeColor),
            StrokeWidth = config.NameCloud.NameStrokeWidth
        };

        NameCloud.Add(item);
        NameAdded?.Invoke(item);

        AudioService.PlayNameAppear();

        if (AttendeeCount % config.NameCloud.SpecialHighlightEvery == 0)
        {
            AudioService.PlayNameHighlight();
            TriggerHighlight();
        }

        while (NameCloud.Count > config.NameCloud.MaxNamesVisible)
        {
            var removed = NameCloud[0];
            NameRemoved?.Invoke(removed);
            NameCloud.RemoveAt(0);
        }
    }

    private void TriggerHighlight()
    {
        foreach (var item in NameCloud)
            item.IsHighlight = true;

        System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
        {
            foreach (var item in NameCloud)
                item.IsHighlight = false;
        });
    }

    public void ToggleDebug() => IsDebugVisible = !IsDebugVisible;

    public void LoadExistingAttendees(List<Attendee> attendees)
    {
        foreach (var attendee in attendees)
        {
            _allAttendees.Add(attendee);
            AddToCloud(attendee.FullName);
        }
        AttendeeCount = _allAttendees.Count;
        LoggingService.Log($"Loaded {attendees.Count} existing attendees to cloud");
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
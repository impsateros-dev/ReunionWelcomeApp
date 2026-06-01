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
          LoggingService.Log($"AddName called with: {name}");
          var attendee = new Attendee { FullName = name };
          _allAttendees.Add(attendee);
          AttendeeCount = _allAttendees.Count;
          LoggingService.Log($"Attendee count now: {AttendeeCount}");

          System.Threading.Tasks.Task.Run(() => ExcelExportService.SaveAttendees(_allAttendees));
          LoggingService.Log($"Name added to cloud: {name}");

          AddToCloud(name);
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

     private void AddToCloudRandom(string name)
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

      private void AddToCloud(string name)
      {
          var config = ConfigService.GetConfig();

          // Show new name temporarily
          ShowNewName(name);

          // Reorder all names alphabetically and layout in grid
          LayoutNamesInGrid();

          // Trigger highlight if needed (every N names)
          if (AttendeeCount % config.NameCloud.SpecialHighlightEvery == 0)
          {
              AudioService.PlayNameHighlight();
              TriggerHighlight();
          }

          // Enforce maximum visible names limit
          while (NameCloud.Count > config.NameCloud.MaxNamesVisible)
          {
              var removed = NameCloud[0];
              NameRemoved?.Invoke(removed);
              NameCloud.RemoveAt(0);
          }
      }

      private void LayoutNamesInGrid()
      {
          LoggingService.Log($"LayoutNamesInGrid called. Window size: {_windowWidth}x{_windowHeight}, Attendee count: {_allAttendees.Count}");
          
          // Ensure we have valid window dimensions
          if (_windowWidth <= 0 || _windowHeight <= 0)
          {
              // Use constructor defaults if window size not yet set
              _windowWidth = 1920;
              _windowHeight = 1080;
              LoggingService.Log($"LayoutNamesInGrid: Using default window size {_windowWidth}x{_windowHeight}");
          }

          var config = ConfigService.GetConfig();

          // Sort names alphabetically
          var sortedNames = _allAttendees.OrderBy(a => a.FullName).ToList();
          LoggingService.Log($"Sorted names count: {sortedNames.Count}");

          // If no names, clear the cloud and exit
          if (sortedNames.Count == 0)
          {
              NameCloud.Clear();
              NamesRepositionRequested?.Invoke();
              LoggingService.Log("No names to display, cleared cloud");
              return;
          }

           // Calculate optimal layout
           int columnCount = GetColumnCount(sortedNames.Count);
           double fontSize = CalculateOptimalFontSize(sortedNames, columnCount);
           LoggingService.Log($"Layout: {columnCount} columns, font size: {fontSize}");

            // Update all name items
            for (int i = 0; i < sortedNames.Count; i++)
            {
                // Find existing item or create new one
                var existingItem = NameCloud.FirstOrDefault(item => item.Name == sortedNames[i].FullName);
                NameItem item;
                
                if (existingItem != null)
                {
                    item = existingItem;
                    // Ensure the name is set correctly (in case it was changed or was incorrect)
                    item.Name = sortedNames[i].FullName;
                }
                else
                {
                    item = new NameItem();
                    item.Name = sortedNames[i].FullName;
                    NameCloud.Add(item);
                    NameAdded?.Invoke(item);
                    AudioService.PlayNameAppear();
                }

                  // Calculate grid position - fill columns top to bottom, then left to right
                // Distribute names as evenly as possible across columns
                int baseItemsPerColumn = sortedNames.Count / columnCount;
                int remainder = sortedNames.Count % columnCount;
                
                // Calculate which column and row this item belongs to
                int columnIndex = 0;
                int rowIndex = 0;
                int itemIndex = 0;
                
                for (int col = 0; col < columnCount; col++)
                {
                    int colItems = baseItemsPerColumn + (col < remainder ? 1 : 0);
                    if (i < itemIndex + colItems)
                    {
                        columnIndex = col;
                        rowIndex = i - itemIndex;
                        break;
                    }
                    itemIndex += colItems;
                }
                
                // Calculate usable space (leave 5% margin on each side)
                double marginFraction = 0.05; // 5% each side
                double usableWidth = _windowWidth * (1.0 - 2 * marginFraction);
                double usableHeight = _windowHeight * (1.0 - 2 * marginFraction);
                
                // Ensure we have usable space
                if (usableWidth <= 0 || usableHeight <= 0)
                {
                    usableWidth = Math.Max(100, _windowWidth - 40); // Fallback
                    usableHeight = Math.Max(100, _windowHeight - 60); // Fallback
                }
                
                // Calculate cell dimensions
                double cellWidth = usableWidth / columnCount;
                double cellHeight = fontSize * 1.5; // fontSize + spacing between rows
                
                // Calculate total grid dimensions
                double gridWidth = columnCount * cellWidth;
                double gridHeight = Math.Ceiling((double)sortedNames.Count / columnCount) * cellHeight;
                
                // Calculate offsets to center the grid in the window
                double offsetX = (_windowWidth - gridWidth) / 2;
                double offsetY = (_windowHeight - gridHeight) / 2;
                
                 // Position items in their grid cells (top-left corner)
                 // TextBlock will be centered horizontally in the View
                 item.X = offsetX + (columnIndex * cellWidth);
                 item.Y = offsetY + (rowIndex * cellHeight);
                item.Size = fontSize;
              
              LoggingService.Log($"Name {i}:{columnIndex}-{rowIndex} {sortedNames[i].FullName} at ({item.X}, {item.Y}), size: {item.Size}");

              // Apply styling from config
              var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(config.NameCloud.NameColor);
              var strokeColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(config.NameCloud.NameStrokeColor);
              
              item.ColorBrush = new SolidColorBrush(color);
              item.StrokeBrush = new SolidColorBrush(strokeColor);
              item.StrokeWidth = config.NameCloud.NameStrokeWidth;
          }

          // Remove excess items if we have more items than names (shouldn't happen but safety check)
          while (NameCloud.Count > sortedNames.Count)
          {
              var removed = NameCloud[NameCloud.Count - 1];
              NameRemoved?.Invoke(removed);
              NameCloud.RemoveAt(NameCloud.Count - 1);
          }

          // Notify UI of layout changes
          NamesRepositionRequested?.Invoke();
          LoggingService.Log($"LayoutNamesInGrid completed. NameCloud count: {NameCloud.Count}");
      }

     private int GetColumnCount(int nameCount)
     {
         // Use 2 columns for 20 or fewer names, 4 columns for more than 20 names
         return nameCount <= 20 ? 2 : 4;
     }

         private double CalculateOptimalFontSize(List<Attendee> sortedNames, int columnCount)
         {
             var config = ConfigService.GetConfig();
             double maxSize = config.NameCloud.NameMaxSize;
             double minSize = config.NameCloud.NameMinSize;

             if (sortedNames.Count == 0)
                 return maxSize;

             // Calculate available space (using 5% margin on each side = 10% total margin)
             double marginFraction = 0.05; // 5% each side
             double usableWidth = _windowWidth * (1.0 - 2 * marginFraction);
             double usableHeight = _windowHeight * (1.0 - 2 * marginFraction);
             
             // Ensure we have usable space
             if (usableWidth <= 0 || usableHeight <= 0)
             {
                 usableWidth = Math.Max(100, _windowWidth - 40); // Fallback
                 usableHeight = Math.Max(100, _windowHeight - 60); // Fallback
             }

             // Calculate rows needed per column
             int rowsPerColumn = (int)Math.Ceiling((double)sortedNames.Count / columnCount);
             
             // VERTICAL CONSTRAINT: Estimate height needed based on font size
             // Each row takes approximately fontSize * 1.5 (including spacing)
             double heightPerRow = maxSize * 1.5;
             double totalHeightNeeded = rowsPerColumn * heightPerRow;
             
             double verticalFontSize = maxSize;
             if (totalHeightNeeded > usableHeight)
             {
                 // Need to reduce font size to fit vertically
                 double scaleFactor = usableHeight / totalHeightNeeded;
                 verticalFontSize = Math.Max(minSize, maxSize * scaleFactor);
             }

             // HORIZONTAL CONSTRAINT: Check if names fit horizontally in columns
             double columnWidth = usableWidth / columnCount;
             
             // Find the longest name to estimate width needed
             double maxNameLength = 0;
             foreach (var attendee in sortedNames)
             {
                 // Conservative estimate: each character takes about 0.5 * fontSize width
                 // This is an approximation - actual width depends on font characteristics
                 // We use maxSize as an upper bound for estimation
                 double nameWidth = attendee.FullName.Length * 0.5 * maxSize;
                 if (nameWidth > maxNameLength)
                     maxNameLength = nameWidth;
             }
             
             // Add significant padding for the name itself (bounding box) and safety margin
             maxNameLength *= 1.5; // 50% extra padding
             
             double horizontalFontSize = maxSize;
             if (maxNameLength > columnWidth)
             {
                 // Need to reduce font size to fit horizontally
                 double scaleFactor = columnWidth / maxNameLength;
                 horizontalFontSize = Math.Max(minSize, maxSize * scaleFactor);
             }

             // Return the smaller of the two constraints to ensure we fit in both directions
             return Math.Min(verticalFontSize, horizontalFontSize);
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
         }
         AttendeeCount = _allAttendees.Count;
         
         // Layout all names in grid format
         LayoutNamesInGrid();
         
         LoggingService.Log($"Loaded {attendees.Count} existing attendees to cloud");
     }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using ReunionWelcomeApp.Services;
using ReunionWelcomeApp.ViewModels;

namespace ReunionWelcomeApp;

public partial class PresentationWindow : Window
{
    public PresentationViewModel ViewModel { get; }
    public event Action<string>? NameReceived;
    private readonly Dictionary<NameItem, TextBlock> _nameElements = new();

    public PresentationWindow()
    {
        InitializeComponent();
        ViewModel = new PresentationViewModel();
        DataContext = ViewModel;

        ViewModel.NameAdded += OnNameItemAdded;
        ViewModel.NameRemoved += OnNameItemRemoved;
        ViewModel.NamesRepositionRequested += OnNamesRepositionRequested;
        
        // Subscribe to SizeChanged event to update ViewModel when window size changes
        SizeChanged += Window_SizeChanged;
        
        try
        {
            var config = ConfigService.GetConfig();
            
            try
            {
                var logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                    config.Images.LogoBlue);
                if (System.IO.File.Exists(logoPath))
                    LogoImage.Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(logoPath));
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to load logo", ex);
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("PresentationWindow initialization error", ex);
        }

        KeyDown += (s, e) =>
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.F1)
                    ViewModel?.ToggleDebug();
                else if (e.Key == System.Windows.Input.Key.F2)
                    ToggleFullscreen();
            }
            catch (Exception ex)
            {
                LoggingService.LogError("KeyDown handler error", ex);
            }
        };

        Loaded += (s, e) =>
        {
            try
            {
                if (ActualWidth > 0 && ActualHeight > 0)
                    ViewModel?.SetWindowSize(ActualWidth, ActualHeight);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Window loaded handler error", ex);
            }
        };
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        try
        {
            if (ActualWidth > 0 && ActualHeight > 0)
            {
                ViewModel?.SetWindowSize(ActualWidth, ActualHeight);
                ViewModel?.RepositionNames(ActualWidth, ActualHeight);
                UpdateCanvasPositions();
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("SizeChanged handler error", ex);
        }
    }

    private void OnNamesRepositionRequested()
    {
        try
        {
            Dispatcher.BeginInvoke(UpdateCanvasPositions);
        }
        catch (Exception ex)
        {
            LoggingService.LogError("OnNamesRepositionRequested error", ex);
        }
    }

    private void UpdateCanvasPositions()
    {
        LoggingService.Log($"UpdateCanvasPositions called. Count: {_nameElements.Count}");
        foreach (var kvp in _nameElements)
        {
            var item = kvp.Key;
            var textBlock = kvp.Value;
            Canvas.SetLeft(textBlock, item.X);
            Canvas.SetTop(textBlock, item.Y);
            LoggingService.Log($"Positioned item '{item.Name}' at ({item.X}, {item.Y})");
        }
    }

    public void ReceiveName(string name)
    {
        try
        {
            Dispatcher.BeginInvoke(() => ViewModel?.AddName(name));
        }
        catch (Exception ex)
        {
            LoggingService.LogError("ReceiveName error", ex);
        }
    }

  private void ToggleFullscreen()
  {
    try
    {
      if (WindowStyle == WindowStyle.None)
      {
        // Exit fullscreen
        WindowStyle = WindowStyle.SingleBorderWindow;
        WindowState = WindowState.Normal;
        Topmost = false;
      }
      else
      {
        // Enter fullscreen
        WindowStyle = WindowStyle.None;
        WindowState = WindowState.Normal;
        ResizeMode = ResizeMode.NoResize;
        Topmost = true;

        var bounds = ScreenService.GetPresentationBounds();

        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
      }
    }
    catch (Exception ex)
    {
      LoggingService.LogError("ToggleFullscreen error", ex);
    }
  }

    private void OnNameItemAdded(NameItem item)
    {
        try
        {
            Dispatcher.BeginInvoke(() =>
            {
                LoggingService.Log($"Creating TextBlock for name: {item.Name}, X: {item.X}, Y: {item.Y}, Size: {item.Size}");
                
                var textBlock = new TextBlock
                {
                    Text = item.Name,
                    FontSize = item.Size,
                    Foreground = item.ColorBrush,
                    FontWeight = FontWeights.Bold,
                    Effect = new DropShadowEffect 
                    { 
                        Color = item.StrokeBrush.Color, 
                        BlurRadius = item.StrokeWidth * 3, 
                        ShadowDepth = 0, 
                        Opacity = 0.8 
                    }
                };
                Canvas.SetLeft(textBlock, item.X);
                Canvas.SetTop(textBlock, item.Y);
                NameCloudCanvas.Children.Add(textBlock);
                _nameElements[item] = textBlock;
                
                // Subscribe to property changes to update the UI when item properties change
                item.PropertyChanged += OnNameItemPropertyChanged;
                
                LoggingService.Log($"TextBlock created and added to canvas. Actual position: ({Canvas.GetLeft(textBlock)}, {Canvas.GetTop(textBlock)})");
            });
        }
        catch (Exception ex)
        {
            LoggingService.LogError("OnNameItemAdded error", ex);
        }
    }

    private void OnNameItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        try
        {
            if (sender is NameItem item && _nameElements.TryGetValue(item, out var textBlock))
            {
                Dispatcher.BeginInvoke(() =>
                {
                    // Update properties that have changed
                    if (e.PropertyName == nameof(NameItem.X))
                    {
                        Canvas.SetLeft(textBlock, item.X);
                    }
                    else if (e.PropertyName == nameof(NameItem.Y))
                    {
                        Canvas.SetTop(textBlock, item.Y);
                    }
                    else if (e.PropertyName == nameof(NameItem.Size))
                    {
                        textBlock.FontSize = item.Size;
                        // Update the effect to account for potential size changes in stroke
                        var effect = textBlock.Effect as DropShadowEffect;
                        if (effect != null)
                        {
                            effect.BlurRadius = item.StrokeWidth * 3;
                        }
                    }
                    else if (e.PropertyName == nameof(NameItem.Name))
                    {
                        textBlock.Text = item.Name;
                    }
                    else if (e.PropertyName == nameof(NameItem.ColorBrush))
                    {
                        textBlock.Foreground = item.ColorBrush;
                    }
                    else if (e.PropertyName == nameof(NameItem.StrokeBrush))
                    {
                        var effect = textBlock.Effect as DropShadowEffect;
                        if (effect != null)
                        {
                            effect.Color = item.StrokeBrush.Color;
                        }
                    }
                    else if (e.PropertyName == nameof(NameItem.StrokeWidth))
                    {
                        var effect = textBlock.Effect as DropShadowEffect;
                        if (effect != null)
                        {
                            effect.BlurRadius = item.StrokeWidth * 3;
                        }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("OnNameItemPropertyChanged error", ex);
        }
    }

    private void OnNameItemRemoved(NameItem item)
    {
        try
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (_nameElements.TryGetValue(item, out var textBlock))
                {
                    // Unsubscribe from property changed event to prevent memory leaks
                    item.PropertyChanged -= OnNameItemPropertyChanged;
                    
                    NameCloudCanvas.Children.Remove(textBlock);
                    _nameElements.Remove(item);
                    textBlock = null;
                }
            });
        }
        catch (Exception ex)
        {
            LoggingService.LogError("OnNameItemRemoved error", ex);
        }
    }
}
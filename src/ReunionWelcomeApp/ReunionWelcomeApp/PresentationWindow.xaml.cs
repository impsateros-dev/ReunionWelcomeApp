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

            try
            {
                var bgPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "Resources/Images/Impsat-FondoBlanco.jpg");
                if (System.IO.File.Exists(bgPath))
                {
                    var bgImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(bgPath));
                    BackgroundImage.Source = bgImage;
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("Failed to load background image", ex);
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
                ViewModel?.RepositionNames(ActualWidth, ActualHeight);
        }
        catch (Exception ex)
        {
            LoggingService.LogError("SizeChanged handler error", ex);
        }
    }

    public void ReceiveName(string name)
    {
        try
        {
            Dispatcher.Invoke(() => ViewModel?.AddName(name));
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
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
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
            Dispatcher.Invoke(() =>
            {
                var textBlock = new TextBlock
                {
                    Text = item.Name,
                    FontSize = item.Size,
                    Foreground = item.ColorBrush,
                    FontWeight = FontWeights.SemiBold,
                    Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 3, ShadowDepth = 1, Opacity = 0.3 }
                };
                Canvas.SetLeft(textBlock, item.X);
                Canvas.SetTop(textBlock, item.Y);
                NameCloudCanvas.Children.Add(textBlock);
                _nameElements[item] = textBlock;
            });
        }
        catch (Exception ex)
        {
            LoggingService.LogError("OnNameItemAdded error", ex);
        }
    }
}
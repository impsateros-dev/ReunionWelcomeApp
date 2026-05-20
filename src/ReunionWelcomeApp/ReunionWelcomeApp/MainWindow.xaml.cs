using System;
using System.Windows;
using System.Windows.Input;
using ReunionWelcomeApp.Services;
using ReunionWelcomeApp.ViewModels;

namespace ReunionWelcomeApp;

public partial class MainWindow : Window
{
    private readonly InputViewModel _viewModel;
    private PresentationWindow? _presentationWindow;
    private bool _isFallbackMode;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new InputViewModel();
        DataContext = _viewModel;

        _viewModel.NameSubmitted += OnNameSubmitted;

        KeyDown += OnKeyDown;
        Loaded += OnLoaded;

        LoggingService.Log("Application started");
    }

    private void LoadExistingAttendees()
    {
        try
        {
            var attendees = ExcelExportService.LoadAttendees();
            if (attendees.Count > 0 && _presentationWindow != null)
            {
                _presentationWindow.ViewModel.LoadExistingAttendees(attendees);
                _viewModel.AttendeeCount = attendees.Count;
                LoggingService.Log($"Loaded {attendees.Count} existing attendees from CSV");
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Failed to load existing attendees", ex);
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ScreenService.HasSecondaryScreen())
        {
            _isFallbackMode = false;
            OpenPresentationWindow();
            LoggingService.Log("Secondary screen detected - presentation window opened");
        }
        else
        {
            _isFallbackMode = true;
            OpenFallbackPresentationWindow();
            LoggingService.LogWarning("No secondary screen - using fallback window on same screen");
        }
        LoadExistingAttendees();
    }

    private void OpenFallbackPresentationWindow()
    {
        try
        {
            _presentationWindow = new PresentationWindow();
            _presentationWindow.WindowStyle = WindowStyle.SingleBorderWindow;
            _presentationWindow.ResizeMode = ResizeMode.CanResize;
            _presentationWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            _presentationWindow.Left = this.Left + 100;
            _presentationWindow.Top = this.Top + 100;
            _presentationWindow.Width = SystemParameters.PrimaryScreenWidth - 200;
            _presentationWindow.Height = SystemParameters.PrimaryScreenHeight - 200;
            _presentationWindow.Show();
            _presentationWindow.Activate();
            this.Activate();
            LoggingService.Log("Fallback presentation window opened on same screen (resizable)");
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Failed to open fallback presentation window", ex);
        }
    }

  private void OpenPresentationWindow()
  {
    try
    {
      var bounds = ScreenService.GetPresentationBounds();

      _presentationWindow = new PresentationWindow
      {
        WindowStartupLocation = WindowStartupLocation.Manual,
        Left = bounds.Left,
        Top = bounds.Top,
        Width = bounds.Width,
        Height = bounds.Height,
        WindowState = WindowState.Normal // critical
      };

      _presentationWindow.Show();

      // Move to correct screen BEFORE maximizing
      _presentationWindow.Left = bounds.Left;
      _presentationWindow.Top = bounds.Top;

      // Now maximize
      _presentationWindow.WindowState = WindowState.Maximized;

      LoggingService.Log("Presentation window opened on secondary screen");
    }
    catch (Exception ex)
    {
      LoggingService.LogError("Failed to open presentation window", ex);
    }
  }

  private void OnNameSubmitted(string name)
    {
        if (_presentationWindow != null)
            _presentationWindow.ReceiveName(name);
        else
            LoggingService.LogWarning("No presentation window available");
    }

private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.EnterPressedCommand.Execute(null);
            if (_viewModel.IsInputActive)
            {
                Dispatcher.BeginInvoke(new Action(() => NameInput.Focus()));
            }
        }
        else if (e.Key == Key.F1)
            ShowDebug();
        else if (e.Key == Key.F2)
            ToggleFullscreen();
        else if (e.Key == Key.F3)
            _viewModel.ToggleDemoCommand.Execute(null);
        else if (e.Key == Key.F4)
            TogglePresentationWindow();
        else if (e.Key == Key.F5)
            RotatePresentationScreen();
    }

    private void RotatePresentationScreen()
    {
        try
        {
            var bounds = ScreenService.RotateToNextScreen();
            if (_presentationWindow != null)
            {
                _presentationWindow.Left = bounds.Left;
                _presentationWindow.Top = bounds.Top;
                _presentationWindow.Width = bounds.Width;
                _presentationWindow.Height = bounds.Height;
                _presentationWindow.WindowState = WindowState.Maximized;
                _presentationWindow.Activate();
                LoggingService.Log("Presentation screen rotated to next display");
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("RotatePresentationScreen error", ex);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (_presentationWindow != null && _presentationWindow.IsVisible)
        {
            _presentationWindow.Close();
            LoggingService.Log("Presentation window closed (input window closed)");
        }
    }

    private void TogglePresentationWindow()
    {
        try
        {
            if (_presentationWindow != null && _presentationWindow.IsVisible)
            {
                _presentationWindow.Hide();
                LoggingService.Log("Presentation window hidden");
            }
            else if (_presentationWindow != null)
            {
                _presentationWindow.Show();
                _presentationWindow.Activate();
                LoggingService.Log("Presentation window shown");
            }
            else
            {
                if (_isFallbackMode)
                    OpenFallbackPresentationWindow();
                else
                    OpenPresentationWindow();
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("TogglePresentationWindow error", ex);
        }
    }

    private void ShowDebug()
    {
        System.Windows.MessageBox.Show(
            $"Attendees: {_viewModel.AttendeeCount}\nDemo Mode: {_viewModel.IsDemoMode}",
            "Debug Info",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ToggleFullscreen()
    {
        if (WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;
        else
            WindowState = WindowState.Maximized;
    }
}
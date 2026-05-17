using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ReunionWelcomeApp.Services;

namespace ReunionWelcomeApp;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (s, args) =>
        {
            LoggingService.LogError("Unhandled UI exception", args.Exception);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            LoggingService.LogError("Unhandled domain exception", ex);
            if (args.IsTerminating)
                LoggingService.Log("Application terminating due to unhandled exception");
        };

        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            LoggingService.LogError("Unobserved task exception", args.Exception);
            args.SetObserved();
        };

        LoggingService.Log("Application started with global exception handlers");
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
            return visibility == Visibility.Visible;
        return false;
    }
}


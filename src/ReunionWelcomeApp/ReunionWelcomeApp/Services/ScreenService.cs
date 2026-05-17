using System;
using System.Windows;
using System.Windows.Forms;
using ReunionWelcomeApp.Services;

namespace ReunionWelcomeApp.Services;

public static class ScreenService
{
    public static Screen? GetSecondaryScreen()
    {
        try
        {
            var screens = Screen.AllScreens;
            if (screens.Length > 1)
            {
                var primary = Screen.PrimaryScreen;
                return screens.FirstOrDefault(s => s.DeviceName != primary?.DeviceName) ?? screens[1];
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Failed to detect screens", ex);
        }
        return null;
    }

    public static Rect GetPresentationBounds()
    {
        var secondary = GetSecondaryScreen();
        if (secondary != null)
        {
            return new Rect(
                secondary.WorkingArea.X,
                secondary.WorkingArea.Y,
                secondary.WorkingArea.Width,
                secondary.WorkingArea.Height);
        }
        return SystemParameters.WorkArea;
    }

    public static bool HasSecondaryScreen() => Screen.AllScreens.Length > 1;
}
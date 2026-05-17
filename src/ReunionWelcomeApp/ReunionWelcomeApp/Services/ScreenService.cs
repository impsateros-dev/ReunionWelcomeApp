using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using ReunionWelcomeApp.Services;

namespace ReunionWelcomeApp.Services;

public static class ScreenService
{
    private static int _currentScreenIndex = -1;

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

    public static List<Screen> GetAllScreens()
    {
        return Screen.AllScreens.ToList();
    }

    public static Rect RotateToNextScreen()
    {
        var screens = Screen.AllScreens;
        if (screens.Length <= 1)
            return SystemParameters.WorkArea;

        if (_currentScreenIndex < 0)
            _currentScreenIndex = 1;
        else
            _currentScreenIndex = (_currentScreenIndex + 1) % screens.Length;

        var screen = screens[_currentScreenIndex];
        LoggingService.Log($"Rotated to screen {_currentScreenIndex}: {screen.DeviceName}");

        return new Rect(
            screen.WorkingArea.X,
            screen.WorkingArea.Y,
            screen.WorkingArea.Width,
            screen.WorkingArea.Height);
    }

    public static void ResetScreenIndex() => _currentScreenIndex = -1;
}
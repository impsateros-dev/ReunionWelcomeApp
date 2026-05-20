using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

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

  public static bool HasSecondaryScreen() => Screen.AllScreens.Length > 1;


  public static Screen GetPreferredScreen()
  {
    try
    {
      var screens = Screen.AllScreens;

      // If only one screen → return primary
      if (screens.Length == 1)
        return screens[0];

      // Prefer any non-primary screen
      return screens.FirstOrDefault(s => !s.Primary) ?? screens[0];
    }
    catch (Exception ex)
    {
      LoggingService.LogError("Failed to detect screens", ex);
      return Screen.PrimaryScreen!;
    }
  }

  public static Rect GetPresentationBounds()
  {
    var screen = GetPreferredScreen();

    // IMPORTANT: use Bounds (full monitor), not WorkingArea
    return new Rect(
        screen.Bounds.X,
        screen.Bounds.Y,
        screen.Bounds.Width,
        screen.Bounds.Height
    );
  }

  public static bool HasMultipleScreens() =>
      Screen.AllScreens.Length > 1;

  public static List<Screen> GetAllScreens() =>
      Screen.AllScreens.ToList();

  public static Rect RotateToNextScreen()
  {
    var screens = Screen.AllScreens;

    if (screens.Length == 0)
      return Rect.Empty;

    _currentScreenIndex = (_currentScreenIndex + 1) % screens.Length;

    var screen = screens[_currentScreenIndex];

    LoggingService.Log($"Switched to screen {_currentScreenIndex}: {screen.DeviceName}");

    return new Rect(
        screen.Bounds.X,
        screen.Bounds.Y,
        screen.Bounds.Width,
        screen.Bounds.Height
    );
  }

  public static void ResetScreenIndex() =>
      _currentScreenIndex = -1;
}
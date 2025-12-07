using Microsoft.Maui.ApplicationModel;

namespace Spazierengeher.Services;

/// <summary>
/// Service zur Erkennung und Verwaltung des App-Themes (Light/Dark Mode)
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gibt das aktuelle System-Theme zurück
    /// </summary>
    AppTheme CurrentTheme { get; }

    /// <summary>
    /// Event wenn sich das Theme ändert
    /// </summary>
    event EventHandler<AppTheme> ThemeChanged;

    /// <summary>
    /// Prüft ob Dark Mode aktiv ist
    /// </summary>
    bool IsDarkMode { get; }
}

public class ThemeService : IThemeService
{
    public AppTheme CurrentTheme => Application.Current?.RequestedTheme ?? AppTheme.Light;

    public bool IsDarkMode => CurrentTheme == AppTheme.Dark;

    public event EventHandler<AppTheme> ThemeChanged;

    public ThemeService()
    {
        // Reagiere auf Theme-Änderungen (z.B. wenn User System-Theme ändert)
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged += (s, e) =>
            {
                ThemeChanged?.Invoke(this, e.RequestedTheme);
            };
        }
    }
}
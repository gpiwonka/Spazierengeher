namespace Spazierengeher.Services;

/// <summary>
/// Interface für Background/Foreground Tracking Service
/// </summary>
public interface IBackgroundTrackingService
{
    /// <summary>
    /// Startet das Background-Tracking
    /// </summary>
    Task StartBackgroundTrackingAsync();

    /// <summary>
    /// Stoppt das Background-Tracking
    /// </summary>
    Task StopBackgroundTrackingAsync();

    /// <summary>
    /// Prüft ob Background-Tracking aktiv ist
    /// </summary>
    bool IsBackgroundTrackingActive { get; }

    /// <summary>
    /// Prüft ob die Berechtigungen für Background-Tracking erteilt wurden
    /// </summary>
    Task<bool> CheckBackgroundPermissionsAsync();

    /// <summary>
    /// Fordert die Berechtigungen für Background-Tracking an
    /// </summary>
    Task<bool> RequestBackgroundPermissionsAsync();
}
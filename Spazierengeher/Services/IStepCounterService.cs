
namespace Spazierengeher.Services;


public interface IStepCounterService
{
    /// <summary>
    /// Startet die Schrittzählung
    /// </summary>
    Task StartCountingAsync();

    /// <summary>
    /// Stoppt die Schrittzählung
    /// </summary>
    Task StopCountingAsync();

    /// <summary>
    /// Gibt die aktuelle Schrittanzahl zurück
    /// </summary>
    Task<int> GetStepCountAsync();

    /// <summary>
    /// Event wird ausgelöst wenn neue Schritte gezählt wurden
    /// </summary>
    event EventHandler<int> StepCountChanged;

    /// <summary>
    /// Prüft ob die Berechtigung für den Schrittzähler erteilt wurde
    /// </summary>
    Task<bool> CheckPermissionAsync();

    /// <summary>
    /// Fordert die Berechtigung für den Schrittzähler an
    /// </summary>
    Task<bool> RequestPermissionAsync();
}
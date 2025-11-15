using Microsoft.Extensions.Hosting;
using Spazierengeher.Data;
using Spazierengeher.Services;

namespace Spazierengeher.BlazorServices;

/// <summary>
/// Blazor-freundlicher Wrapper für den Schrittzähler-Service mit Datenbankintegration
/// VERSION: OHNE NOTIFICATION (Funktioniert zuverlässig)
/// </summary>
public class BlazorStepCounterService
{
    private readonly IStepCounterService _stepCounterService;
    private readonly DailyStepsDb _database;
    private int _currentSteps;
    private int _initialStepsToday;
    private UserSettings _settings;
    private Timer _autoSaveTimer;
    private readonly Task _initializationTask;

    public event EventHandler<int> OnStepCountChanged;

    public int CurrentSteps => _currentSteps;
    public bool IsTracking { get; private set; }
    public UserSettings Settings => _settings;

    public BlazorStepCounterService(IStepCounterService stepCounterService, DailyStepsDb database)
    {
        _stepCounterService = stepCounterService;
        _database = database;
        _stepCounterService.StepCountChanged += HandleStepCountChanged;

        // Initialisierung - lädt Einstellungen und heutige Schritte
        _initializationTask = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _settings = await _database.GetSettingsAsync();
        _initialStepsToday = await _database.GetStepsAsync(DailyStepsDb.TodayKey);
        _currentSteps = _initialStepsToday;
    }

    private async void HandleStepCountChanged(object sender, int stepCount)
    {
        await _initializationTask;

        // Kombiniere neue Schritte mit bereits gespeicherten
        _currentSteps = _initialStepsToday + stepCount;

        // Event auf dem Main-Thread auslösen für Blazor
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnStepCountChanged?.Invoke(this, _currentSteps);
        });

        // Automatisch in DB speichern (asynchron ohne await)
        _ = SaveCurrentStepsAsync();
    }

    public async Task<bool> CheckAndRequestPermissionAsync()
    {
        var hasPermission = await _stepCounterService.CheckPermissionAsync();
        if (!hasPermission)
        {
            hasPermission = await _stepCounterService.RequestPermissionAsync();
        }
        return hasPermission;
    }

    public async Task<(bool Success, string Message)> StartTrackingAsync()
    {
        try
        {
            await _initializationTask;
            var hasPermission = await CheckAndRequestPermissionAsync();
            if (!hasPermission)
            {
                return (false, "Berechtigung wurde nicht erteilt. Bitte erlaube den Zugriff in den Einstellungen.");
            }

            // Lade bereits heute gezählte Schritte
            _initialStepsToday = await _database.GetStepsAsync(DailyStepsDb.TodayKey);
            _currentSteps = _initialStepsToday;

            // Starte Schrittzähler-Service
            System.Diagnostics.Debug.WriteLine("🔷 Starte IStepCounterService...");
            await _stepCounterService.StartCountingAsync();
            System.Diagnostics.Debug.WriteLine("✅ IStepCounterService gestartet");

            IsTracking = true;

            // Starte Auto-Save Timer (alle 30 Sekunden)
            _autoSaveTimer?.Dispose();
            _autoSaveTimer = new Timer(async _ => await SaveCurrentStepsAsync(), null,
                TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

            return (true, "Tracking gestartet");
        }
        catch (NotSupportedException)
        {
            return (false, "Dein Gerät unterstützt keinen Schrittzähler.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ StartTracking Fehler: {ex}");
            return (false, $"Fehler beim Starten: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> StopTrackingAsync()
    {
        try
        {
            await _initializationTask;
            System.Diagnostics.Debug.WriteLine("🔷 Stoppe IStepCounterService...");
            await _stepCounterService.StopCountingAsync();
            System.Diagnostics.Debug.WriteLine("✅ IStepCounterService gestoppt");

            IsTracking = false;

            // Timer stoppen
            _autoSaveTimer?.Dispose();
            _autoSaveTimer = null;

            // Finale Speicherung
            await SaveCurrentStepsAsync();

            return (true, "Tracking gestoppt");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ StopTracking Fehler: {ex}");
            return (false, $"Fehler beim Stoppen: {ex.Message}");
        }
    }

    public async Task<int> GetCurrentStepsAsync()
    {
        return await _stepCounterService.GetStepCountAsync();
    }

    /// <summary>
    /// Speichert die aktuellen Schritte in der Datenbank
    /// </summary>
    private async Task SaveCurrentStepsAsync()
    {
        try
        {
            await _database.UpsertStepsAsync(DailyStepsDb.TodayKey, _currentSteps);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fehler beim Speichern der Schritte: {ex.Message}");
        }
    }

    /// <summary>
    /// Lädt die Historie der letzten Tage
    /// </summary>
    public async Task<List<DailySteps>> GetRecentHistoryAsync(int days = 14)
    {
        return await _database.GetRecentAsync(days);
    }

    /// <summary>
    /// Aktualisiert die Benutzereinstellungen
    /// </summary>
    public async Task UpdateSettingsAsync(UserSettings settings)
    {
        settings.UpdatedAt = DateTime.Now;
        await _database.SaveSettingsAsync(settings);
        _settings = settings;
    }

    /// <summary>
    /// Lädt die Einstellungen neu
    /// </summary>
    public async Task ReloadSettingsAsync()
    {
        _settings = await _database.GetSettingsAsync();
    }

    // Hilfsmethoden für berechnete Werte (verwenden Benutzereinstellungen)
    public double GetEstimatedCalories()
    {
        // Kalorienberechnung: ~0.04 kcal pro Schritt (Durchschnitt)
        // Genauer: (Gewicht in kg × 0.57) / 1000 Schritte
        if (_settings != null && _settings.WeightKg > 0)
        {
            return (_settings.WeightKg * 0.57 * _currentSteps) / 1000;
        }
        return _currentSteps * 0.04;
    }

    public double GetEstimatedDistanceKm()
    {
        // Verwendet die Schrittlänge aus den Einstellungen
        if (_settings != null && _settings.StepLengthMeters > 0)
        {
            return (_currentSteps * _settings.StepLengthMeters) / 1000;
        }
        // Standard: 80cm Schrittlänge
        return _currentSteps * 0.0008;
    }

    public double GetProgressPercentage()
    {
        int goal = _settings?.DailyGoal ?? 10000;
        return Math.Min((_currentSteps / (double)goal) * 100, 100);
    }

    public int GetDailyGoal()
    {
        return _settings?.DailyGoal ?? 10000;
    }
}
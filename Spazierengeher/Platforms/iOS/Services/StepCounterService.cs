using CoreMotion;
using Foundation;
using Spazierengeher.Services;

namespace Spazierengeher.Platforms.iOS.Services;

/// <summary>
/// iOS-Implementierung des Schrittzählers mit CoreMotion Framework
/// </summary>
public class StepCounterService : IStepCounterService
{
    private CMPedometer _pedometer;
    private int _initialStepCount = 0;
    private int _currentStepCount = 0;
    private bool _isCountingStarted = false;
    private NSDate _startDate;

    public event EventHandler<int> StepCountChanged;

    public StepCounterService()
    {
        _pedometer = new CMPedometer();
    }

    public Task StartCountingAsync()
    {
        if (!_isCountingStarted)
        {
            _startDate = NSDate.Now;
            _currentStepCount = 0;
            _initialStepCount = 0;
            _isCountingStarted = true;

            // Starte Schrittzählung ab jetzt
            _pedometer.StartPedometerUpdates(_startDate, (data, error) =>
            {
                if (error != null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ CoreMotion Fehler: {error.LocalizedDescription}");
                    return;
                }

                if (data != null)
                {
                    _currentStepCount = (int)data.NumberOfSteps.Int32Value;

                    // Event auf Main-Thread auslösen
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StepCountChanged?.Invoke(this, _currentStepCount);
                        System.Diagnostics.Debug.WriteLine($"📱 iOS Schritte: {_currentStepCount}");
                    });
                }
            });

            System.Diagnostics.Debug.WriteLine("✅ iOS Schrittzähler gestartet");
        }

        return Task.CompletedTask;
    }

    public Task StopCountingAsync()
    {
        if (_isCountingStarted)
        {
            _pedometer.StopPedometerUpdates();
            _isCountingStarted = false;
            System.Diagnostics.Debug.WriteLine($"🛑 iOS Schrittzähler gestoppt bei {_currentStepCount} Schritten");
        }

        return Task.CompletedTask;
    }

    public Task<int> GetStepCountAsync()
    {
        return Task.FromResult(_currentStepCount);
    }

    public async Task<bool> CheckPermissionAsync()
    {
        // Prüfe ob CMPedometer verfügbar ist
        if (!CMPedometer.IsStepCountingAvailable)
        {
            System.Diagnostics.Debug.WriteLine("❌ Schrittzählung nicht verfügbar auf diesem Gerät");
            return false;
        }

        // In iOS gibt es keine explizite Permission für Schrittzählung
        // Die Permission wird automatisch beim ersten Zugriff abgefragt
        // Wir können nur prüfen ob das Feature verfügbar ist
        return CMPedometer.IsStepCountingAvailable;
    }

    public async Task<bool> RequestPermissionAsync()
    {
        // iOS fragt automatisch nach Permission beim ersten Zugriff auf CMPedometer
        // Wir müssen nur prüfen ob das Feature verfügbar ist
        if (!CMPedometer.IsStepCountingAvailable)
        {
            System.Diagnostics.Debug.WriteLine("❌ Schrittzählung nicht verfügbar");
            return false;
        }

        // Teste den Zugriff um die Permission-Abfrage zu triggern
        try
        {
            var testDate = NSDate.Now.AddSeconds(-1);
            var tcs = new TaskCompletionSource<bool>();

            _pedometer.QueryPedometerData(testDate, NSDate.Now, (data, error) =>
            {
                if (error != null)
                {
                    // Permission wurde verweigert oder Fehler aufgetreten
                    System.Diagnostics.Debug.WriteLine($"❌ Permission Test Fehler: {error.LocalizedDescription}");
                    tcs.SetResult(false);
                }
                else
                {
                    // Permission wurde erteilt
                    System.Diagnostics.Debug.WriteLine("✅ iOS Motion Permission erteilt");
                    tcs.SetResult(true);
                }
            });

            return await tcs.Task;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ RequestPermission Fehler: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Holt historische Schritt-Daten für einen bestimmten Zeitraum
    /// </summary>
    public async Task<int> GetHistoricalStepsAsync(DateTime startDate, DateTime endDate)
    {
        var tcs = new TaskCompletionSource<int>();

        var nsStartDate = (NSDate)startDate;
        var nsEndDate = (NSDate)endDate;

        _pedometer.QueryPedometerData(nsStartDate, nsEndDate, (data, error) =>
        {
            if (error != null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Historische Daten Fehler: {error.LocalizedDescription}");
                tcs.SetResult(0);
            }
            else if (data != null)
            {
                var steps = (int)data.NumberOfSteps.Int32Value;
                tcs.SetResult(steps);
            }
            else
            {
                tcs.SetResult(0);
            }
        });

        return await tcs.Task;
    }
}
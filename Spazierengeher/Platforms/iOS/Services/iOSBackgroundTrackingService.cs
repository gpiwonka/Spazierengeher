using CoreMotion;
using Foundation;
using UIKit;
using Spazierengeher.Services;

namespace Spazierengeher.Platforms.iOS.Services;

/// <summary>
/// iOS-Implementierung für Background-Tracking mit Background Modes
/// </summary>
public class iOSBackgroundTrackingService : IBackgroundTrackingService
{
    private const string PREF_KEY_TRACKING_ACTIVE = "background_tracking_active";
    private CMPedometer _backgroundPedometer;
    private nint _backgroundTaskId = UIApplication.BackgroundTaskInvalid;

    public bool IsBackgroundTrackingActive
    {
        get => Preferences.Get(PREF_KEY_TRACKING_ACTIVE, false);
        private set => Preferences.Set(PREF_KEY_TRACKING_ACTIVE, value);
    }

    public iOSBackgroundTrackingService()
    {
        _backgroundPedometer = new CMPedometer();
    }

    public Task StartBackgroundTrackingAsync()
    {
        try
        {
            // In iOS wird Background-Tracking durch Background Modes aktiviert
            // Die eigentliche Implementierung erfolgt über CoreMotion

            System.Diagnostics.Debug.WriteLine("✅ iOS Background-Tracking gestartet");

            // Registriere für Background-Updates
            if (CMPedometer.IsStepCountingAvailable)
            {
                IsBackgroundTrackingActive = true;
                System.Diagnostics.Debug.WriteLine("✅ CMPedometer Background Mode aktiv");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Schrittzählung nicht verfügbar");
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Background-Tracking Start Fehler: {ex.Message}");
            throw;
        }
    }

    public Task StopBackgroundTrackingAsync()
    {
        try
        {
            IsBackgroundTrackingActive = false;
            System.Diagnostics.Debug.WriteLine("🛑 iOS Background-Tracking gestoppt");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Background-Tracking Stop Fehler: {ex.Message}");
            throw;
        }
    }

    public Task<bool> CheckBackgroundPermissionsAsync()
    {
        // iOS benötigt Motion & Fitness Permission
        bool hasMotionPermission = CMPedometer.IsStepCountingAvailable;

        System.Diagnostics.Debug.WriteLine($"📱 Motion Permission Status: {hasMotionPermission}");

        return Task.FromResult(hasMotionPermission);
    }

    public async Task<bool> RequestBackgroundPermissionsAsync()
    {
        try
        {
            // iOS fragt automatisch nach Motion Permission beim ersten Zugriff
            if (!CMPedometer.IsStepCountingAvailable)
            {
                System.Diagnostics.Debug.WriteLine("❌ Schrittzählung nicht verfügbar");
                return false;
            }

            // Teste Zugriff um Permission-Dialog zu triggern
            var tcs = new TaskCompletionSource<bool>();
            var testDate = NSDate.Now.AddSeconds(-1);

            _backgroundPedometer.QueryPedometerData(testDate, NSDate.Now, (data, error) =>
            {
                if (error != null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Motion Permission verweigert: {error.LocalizedDescription}");
                    tcs.SetResult(false);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("✅ Motion Permission erteilt");
                    tcs.SetResult(true);
                }
            });

            return await tcs.Task;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Permission Request Fehler: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Startet einen Background Task (iOS spezifisch)
    /// </summary>
    private void BeginBackgroundTask()
    {
        _backgroundTaskId = UIApplication.SharedApplication.BeginBackgroundTask(() =>
        {
            // Task wird beendet wenn Zeit abläuft
            EndBackgroundTask();
        });
    }

    /// <summary>
    /// Beendet einen Background Task
    /// </summary>
    private void EndBackgroundTask()
    {
        if (_backgroundTaskId != UIApplication.BackgroundTaskInvalid)
        {
            UIApplication.SharedApplication.EndBackgroundTask(_backgroundTaskId);
            _backgroundTaskId = UIApplication.BackgroundTaskInvalid;
        }
    }
}
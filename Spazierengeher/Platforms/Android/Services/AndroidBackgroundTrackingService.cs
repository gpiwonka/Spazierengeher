using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Spazierengeher.Services;

namespace Spazierengeher.Platforms.Android.Services;

/// <summary>
/// Android-Implementierung für Background-Tracking via Foreground Service
/// HINWEIS: Dieser Service wird nicht mehr benötigt, da der StepCounterForegroundService
/// jetzt direkt vom BlazorStepCounterService gesteuert wird.
/// </summary>
public class AndroidBackgroundTrackingService : IBackgroundTrackingService
{
    private const string PREF_KEY_TRACKING_ACTIVE = "background_tracking_active";

    public bool IsBackgroundTrackingActive
    {
        get => Preferences.Get(PREF_KEY_TRACKING_ACTIVE, false);
        private set => Preferences.Set(PREF_KEY_TRACKING_ACTIVE, value);
    }

    public Task StartBackgroundTrackingAsync()
    {
        // Diese Methode wird nicht mehr verwendet
        // Das Tracking wird jetzt direkt vom BlazorStepCounterService gesteuert
        IsBackgroundTrackingActive = true;
        return Task.CompletedTask;
    }

    public Task StopBackgroundTrackingAsync()
    {
        // Diese Methode wird nicht mehr verwendet
        IsBackgroundTrackingActive = false;
        return Task.CompletedTask;
    }

    public async Task<bool> CheckBackgroundPermissionsAsync()
    {
        // Android 13+ (API 33) benötigt POST_NOTIFICATIONS Berechtigung
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            var status = await Microsoft.Maui.ApplicationModel.Permissions.CheckStatusAsync<PostNotifications>();
            if (status != PermissionStatus.Granted)
            {
                return false;
            }
        }

        // Android 10+ (API 29) benötigt ACTIVITY_RECOGNITION
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
        {
            var context = Platform.AppContext;
            var permission = context.CheckSelfPermission(global::Android.Manifest.Permission.ActivityRecognition);
            if (permission != Permission.Granted)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> RequestBackgroundPermissionsAsync()
    {
        // POST_NOTIFICATIONS für Android 13+
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            var status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<PostNotifications>();
            if (status != PermissionStatus.Granted)
            {
                System.Diagnostics.Debug.WriteLine("❌ POST_NOTIFICATIONS verweigert!");
                return false;
            }
        }

        // ACTIVITY_RECOGNITION für Android 10+
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
        {
            var context = Platform.AppContext;

            if (Platform.CurrentActivity != null)
            {
                var permissions = new[] { global::Android.Manifest.Permission.ActivityRecognition };
                Platform.CurrentActivity.RequestPermissions(permissions, 1001);

                await Task.Delay(1000);
                var permission = context.CheckSelfPermission(global::Android.Manifest.Permission.ActivityRecognition);
                if (permission != Permission.Granted)
                {
                    return false;
                }
            }
        }

        return true;
    }
}

/// <summary>
/// Custom Permission für Post Notifications (Android 13+)
/// </summary>
public class PostNotifications : Microsoft.Maui.ApplicationModel.Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new[] { (global::Android.Manifest.Permission.PostNotifications, true) };
}
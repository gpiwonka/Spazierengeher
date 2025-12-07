using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using Java.Lang;

namespace Spazierengeher.Platforms.Android.Services;

[Service(ForegroundServiceType = ForegroundService.TypeHealth)]
public class StepCounterForegroundService : Service
{
    private const int NOTIFICATION_ID = 2001;
    private const string CHANNEL_ID = "step_counter_channel";
    private StepUpdateReceiver _receiver;
    private NotificationCompat.Builder _notificationBuilder;
    private int _goal = 10000;

    public override IBinder OnBind(Intent intent) => null;

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        CreateNotificationChannel();
        StartForeground(NOTIFICATION_ID, CreateNotification(0));
        RegisterStepReceiver();

        System.Diagnostics.Debug.WriteLine("✅ ForegroundService gestartet (nur Notification)");

        // Return RedeliverIntent to restart service with same intent if killed
        return StartCommandResult.RedeliverIntent;
    }

    public override void OnTaskRemoved(Intent rootIntent)
    {
        base.OnTaskRemoved(rootIntent);

        // Restart service when app is removed from recent apps
        var restartServiceIntent = new Intent(ApplicationContext, this.Class);
        var restartServicePendingIntent = PendingIntent.GetService(
            ApplicationContext, 1, restartServiceIntent,
            PendingIntentFlags.OneShot | PendingIntentFlags.Immutable);

        var alarmService = (AlarmManager)ApplicationContext.GetSystemService(AlarmService);
        alarmService.Set(AlarmType.ElapsedRealtime,
            SystemClock.ElapsedRealtime() + 1000,
            restartServicePendingIntent);

        System.Diagnostics.Debug.WriteLine("🔄 Service wird neu gestartet nach Task-Entfernung");
    }

    private void RegisterStepReceiver()
    {
        _receiver = new StepUpdateReceiver(this);
        var filter = new IntentFilter("com.spazierengeher.STEPS_UPDATED");
        RegisterReceiver(_receiver, filter);
    }

    private Notification CreateNotification(int steps)
    {
        var intent = PackageManager.GetLaunchIntentForPackage(PackageName);
        var pending = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Immutable);

        var progressPercent = System.Math.Min((int)((steps / (double)_goal) * 100), 100);

        _notificationBuilder = new NotificationCompat.Builder(this, CHANNEL_ID)
            .SetContentTitle("🚶 Spazierengeher")
            .SetContentText($"Heute: {steps:N0} / {_goal:N0} Schritte ({progressPercent}%)")
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetContentIntent(pending)
            .SetOngoing(true)
            .SetOnlyAlertOnce(true)
            .SetCategory(NotificationCompat.CategoryService)
            .SetVisibility(NotificationCompat.VisibilityPublic)
            .SetProgress(_goal, steps, false)
            .SetShowWhen(false);

        return _notificationBuilder.Build();
    }

    public void UpdateNotification(int steps)
    {
        var progressPercent = System.Math.Min((int)((steps / (double)_goal) * 100), 100);

        _notificationBuilder
            .SetContentText($"Heute: {steps:N0} / {_goal:N0} Schritte ({progressPercent}%)")
            .SetProgress(_goal, steps, false);

        var manager = (NotificationManager)GetSystemService(NotificationService);
        manager?.Notify(NOTIFICATION_ID, _notificationBuilder.Build());
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(CHANNEL_ID, "Schrittzähler", NotificationImportance.Low)
            {
                Description = "Zeigt deine Schritte an",
                LockscreenVisibility = NotificationVisibility.Public
            };
            var nm = (NotificationManager)GetSystemService(NotificationService);
            nm.CreateNotificationChannel(channel);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_receiver != null)
            UnregisterReceiver(_receiver);
    }

    public static void StartService(Context context)
    {
        var intent = new Intent(context, typeof(StepCounterForegroundService));
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            context.StartForegroundService(intent);
        else
            context.StartService(intent);
    }

    private class StepUpdateReceiver : BroadcastReceiver
    {
        private readonly StepCounterForegroundService _service;
        public StepUpdateReceiver(StepCounterForegroundService service) => _service = service;

        public override void OnReceive(Context context, Intent intent)
        {
            int steps = intent.GetIntExtra("steps", 0);
            _service.UpdateNotification(steps);
        }
    }
}

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;

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
        return StartCommandResult.Sticky;
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

        _notificationBuilder = new NotificationCompat.Builder(this, CHANNEL_ID)
            .SetContentTitle("🚶 Schritte zählen...")
            .SetContentText($"Heute: {steps:N0} Schritte")
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
            .SetContentIntent(pending)
            .SetOngoing(true)
            .SetOnlyAlertOnce(true)
            .SetCategory(NotificationCompat.CategoryService)
            .SetVisibility(NotificationCompat.VisibilityPublic);

        return _notificationBuilder.Build();
    }

    public void UpdateNotification(int steps)
    {
        _notificationBuilder
            .SetContentText($"Heute: {steps:N0} Schritte")
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

using Android.Content;
using Android.Hardware;
using Spazierengeher.Services;
using And = global::Android;
namespace Spazierengeher.Platforms.Android.Services;

public class StepCounterService : Java.Lang.Object, IStepCounterService, ISensorEventListener
{
    private SensorManager _sensorManager;
    private Sensor _stepCounterSensor;
    private int _initialStepCount = -1;
    private int _currentStepCount = 0;
    private bool _isCountingStarted = false;

    public event EventHandler<int> StepCountChanged;

    public StepCounterService()
    {
        var context = Platform.AppContext;
        _sensorManager = (SensorManager)context.GetSystemService(Context.SensorService);
        _stepCounterSensor = _sensorManager.GetDefaultSensor(SensorType.StepCounter)
            ?? _sensorManager.GetDefaultSensor(SensorType.StepDetector);
    }

    public async Task StartCountingAsync()
    {
        if (_stepCounterSensor == null)
            throw new NotSupportedException("Schrittzähler-Sensor nicht verfügbar auf diesem Gerät");

        if (!_isCountingStarted)
        {
            bool hasPermission = true;

#if ANDROID
            var status = await Permissions.CheckStatusAsync<Permissions.Sensors>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Sensors>();
                if (status != PermissionStatus.Granted)
                {
                   // await DisplayAlert("Fehler", "Bitte Schrittzähler-Berechtigung erlauben.", "OK");
                    return;
                }
            }
#endif

            _sensorManager.RegisterListener(this, _stepCounterSensor, SensorDelay.Normal);
            _isCountingStarted = true;
            _initialStepCount = -1;
        }

        await Task.CompletedTask;
    }

    public Task StopCountingAsync()
    {
        if (_isCountingStarted)
        {
            _sensorManager.UnregisterListener(this);
            _isCountingStarted = false;
        }

        return Task.CompletedTask;
    }

    public Task<int> GetStepCountAsync() => Task.FromResult(_currentStepCount);

    public void OnSensorChanged(SensorEvent e)
    {
        if (e?.Sensor == null) return;

        int steps;
        if (e.Sensor.Type == SensorType.StepCounter)
        {
            int totalSteps = (int)e.Values[0];
            if (_initialStepCount == -1)
            {
                _initialStepCount = totalSteps;
                _currentStepCount = 0;
            }
            else
            {
                _currentStepCount = totalSteps - _initialStepCount;
            }
            steps = _currentStepCount;
        }
        else if (e.Sensor.Type == SensorType.StepDetector)
        {
            _currentStepCount++;
            steps = _currentStepCount;
        }
        else return;

        // 🔔 App-internes Event
        StepCountChanged?.Invoke(this, _currentStepCount);

        // 📡 Broadcast an ForegroundService
        var intent = new Intent("com.spazierengeher.STEPS_UPDATED");
        intent.PutExtra("steps", steps);
        Platform.AppContext.SendBroadcast(intent);

        System.Diagnostics.Debug.WriteLine($"👟 Schritte: {_currentStepCount}");
    }

    public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy) { }

    public async Task<bool> CheckPermissionAsync()
    {
        if (And.OS.Build.VERSION.SdkInt >= And.OS.BuildVersionCodes.Q)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Sensors>();
            return status == PermissionStatus.Granted;
        }
        return true;
    }

    public async Task<bool> RequestPermissionAsync()
    {
        if (And.OS.Build.VERSION.SdkInt >= And.OS.BuildVersionCodes.Q)
        {
            var status = await Permissions.RequestAsync<Permissions.Sensors>();
            return status == PermissionStatus.Granted;
        }
        return true;
    }
}

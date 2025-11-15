using Spazierengeher.Services;
#if ANDROID
using Spazierengeher.Platforms.Android.Services;
#endif
namespace Spazierengeher
{
    public partial class MainPage : ContentPage
    {
        private readonly IStepCounterService _steps;

        public MainPage(IStepCounterService steps)
        {
            InitializeComponent();
            _steps = steps;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

#if ANDROID
            try
            {
                // 1️⃣ ACTIVITY_RECOGNITION Berechtigung prüfen
                var ar = await Permissions.CheckStatusAsync<Spazierengeher.Platforms.Android.ActivityRecognitionPermission>();
                if (ar != PermissionStatus.Granted)
                {
                    ar = await Permissions.RequestAsync<Spazierengeher.Platforms.Android.ActivityRecognitionPermission>();
                    if (ar != PermissionStatus.Granted)
                    {
                        await DisplayAlertAsync("Berechtigung fehlt",
                            "Bitte erlaube 'Aktivitäten erkennen' für den Schrittzähler.", "OK");
                        return;
                    }
                }

                
                await _steps.StartCountingAsync();

                await Task.Delay(1500);

                var context = Platform.AppContext;
                Spazierengeher.Platforms.Android.Services.StepCounterForegroundService.StartService(context);

                System.Diagnostics.Debug.WriteLine("✅ Alles gestartet: Sensor + Notification aktiv");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Fehler", ex.Message, "OK");
                System.Diagnostics.Debug.WriteLine($"❌ Fehler: {ex}");
            }
#endif
        }



        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await _steps.StopCountingAsync();
        }
    }
}

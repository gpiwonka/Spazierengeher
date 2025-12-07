using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Provider;

namespace Spazierengeher
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Request battery optimization exemption for reliable background service
            RequestBatteryOptimizationExemption();
        }

        private void RequestBatteryOptimizationExemption()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var packageName = PackageName;
                var powerManager = (PowerManager)GetSystemService(PowerService);

                if (powerManager != null && !powerManager.IsIgnoringBatteryOptimizations(packageName))
                {
                    try
                    {
                        var intent = new Intent(Settings.ActionRequestIgnoreBatteryOptimizations);
                        intent.SetData(Android.Net.Uri.Parse("package:" + packageName));
                        StartActivity(intent);
                        System.Diagnostics.Debug.WriteLine("✅ Requesting battery optimization exemption");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Battery optimization request failed: {ex.Message}");
                    }
                }
            }
        }
    }
}

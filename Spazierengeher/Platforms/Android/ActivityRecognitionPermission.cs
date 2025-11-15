// z.B. in Platforms/Android/ActivityRecognitionPermission.cs
using Microsoft.Maui.ApplicationModel;

namespace Spazierengeher.Platforms.Android;

public class ActivityRecognitionPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new[] { (global::Android.Manifest.Permission.ActivityRecognition, true) };
}

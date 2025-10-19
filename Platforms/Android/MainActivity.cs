using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace Resto2025
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        const int RequestBluetoothPermission = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestBluetoothPermissions();
        }

        void RequestBluetoothPermissions()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect) != Permission.Granted ||
                    ContextCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothScan) != Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new string[]
                    {
                    Manifest.Permission.BluetoothConnect,
                    Manifest.Permission.BluetoothScan
                    }, RequestBluetoothPermission);
                }
            }
        }
    }
}

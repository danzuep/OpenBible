using Android.App;
using Android.Content.PM;
using Android.OS;

namespace BibleApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public sealed class MainActivity : MauiAppCompatActivity
    {
    }
}

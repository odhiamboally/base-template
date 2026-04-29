using Foundation;

namespace BT.UI.Maui;

[Register("AppDelegate")]
internal sealed class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

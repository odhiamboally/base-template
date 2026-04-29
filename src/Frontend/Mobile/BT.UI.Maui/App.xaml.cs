namespace BT.UI.Maui;

internal sealed partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    //protected override Window CreateWindow(IActivationState? activationState)
    //    => new Window(new MainPage()) { Title = "BT.UI.Maui" };

    protected override Window CreateWindow(IActivationState? _)
        => new Window(new MainPage());
}

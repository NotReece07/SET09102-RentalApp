namespace StarterApp;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = _serviceProvider.GetService<AppShell>();
        if (shell == null)
        {
            // Handle the error if AppShell could not be resolved
            throw new InvalidOperationException("AppShell could not be resolved from the service provider.");
        }

        var window = new Window(shell);
        return window;
    }
}
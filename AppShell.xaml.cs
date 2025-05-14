namespace DockerDesktop;

public partial class AppShell : Shell
{

    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(ContainerDetailPage), typeof(ContainerDetailPage));
    }
}

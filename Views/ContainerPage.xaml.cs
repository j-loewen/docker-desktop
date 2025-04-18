namespace DockerDesktop.Views;

public partial class ContainerPage : ContentPage
{
    public ContainerPage(ContainerViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
    }
}

namespace DockerDesktop.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;

        this.Title = "Settings";
    }
}
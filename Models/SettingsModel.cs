namespace DockerDesktop.Models;

public partial class SettingsModel : ObservableObject
{
    [ObservableProperty]
    private String host = string.Empty;

}
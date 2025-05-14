namespace DockerDesktop.Models;

public partial class NetworkModel : ObservableObject {
    [ObservableProperty]
    private String id = string.Empty;

    [ObservableProperty]
    private String name = string.Empty;

    [ObservableProperty]
    private String driver = string.Empty;
}

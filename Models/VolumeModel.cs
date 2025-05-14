namespace DockerDesktop.Models;

public partial class VolumeModel : ObservableObject {
    [ObservableProperty]
    private String name = string.Empty;

    [ObservableProperty]
    private String driver = string.Empty;
}

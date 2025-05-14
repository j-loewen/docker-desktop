namespace DockerDesktop.Models;

public partial class ImageModel : ObservableObject {
    [ObservableProperty]
    private String id = string.Empty;

    [ObservableProperty]
    private String name = string.Empty;

    [ObservableProperty]
    private long size = 0L;

    [ObservableProperty]
    private DateTime created = DateTime.MinValue;
}

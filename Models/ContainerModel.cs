namespace DockerDesktop.Models;

public partial class ContainerModel : ObservableObject {
    [ObservableProperty]
    private String id = string.Empty;

    [ObservableProperty]
    private String name = string.Empty;

    [ObservableProperty]
    private String state = string.Empty;

    [ObservableProperty]
    private DateTime created = DateTime.MinValue;

    [ObservableProperty]
    private String command = string.Empty;

    [ObservableProperty]
    private ImageModel image;

    [ObservableProperty]
    private String status = string.Empty;

    [ObservableProperty]
    private ObservableCollection<KeyValueModel> labels = new();

    [ObservableProperty]
    private NetworkModel network;
}
namespace DockerDesktop.Models;

public partial class KeyValueModel : ObservableObject {
    [ObservableProperty]
    private String key = string.Empty;

    [ObservableProperty]
    private String value = string.Empty;
}

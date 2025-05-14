namespace DockerDesktop.Models;

public partial class SystemInfoModel : ObservableObject {
    [ObservableProperty]
    private String os = string.Empty;

    [ObservableProperty]
    private String architecture = string.Empty;

    [ObservableProperty]
    private int cpuCores = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MemoryInGB))]
    private long memory = 0L;

    public long MemoryInGB => (this.Memory / 1000 / 1000 / 1000);

    [ObservableProperty]
    private String name = String.Empty;

    [ObservableProperty]
    private Int32 containers = 0;

    [ObservableProperty]
    private Int32 containersRunning = 0;

    [ObservableProperty]
    private Int32 containersStopped = 0;

    [ObservableProperty]
    private Int32 containersPaused = 0;

    [ObservableProperty]
    private Int32 images = 0;
}

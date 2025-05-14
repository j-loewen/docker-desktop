namespace DockerDesktop.ViewModels;

public partial class ContainerViewModel : BaseViewModel {
    private readonly DockerService dockerService;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private String serviceState = "Disconnected";

    public ObservableCollection<ContainerModel> FilteredContainers { get; } = [];

    [ObservableProperty]
    private SystemInfoModel systemInfo = new SystemInfoModel();

    [ObservableProperty]
    private ContainerModel? selectedContainer;

    [ObservableProperty]
    private Int32 collectionViewItemsLayoutSpan = 3;


    public ContainerViewModel(DockerService dockerService) {
        this.Title = "Container";
        this.dockerService = dockerService;

        // Monitor container collection changes
        this.dockerService.Container.CollectionChanged += (s, e) => {
            MainThread.BeginInvokeOnMainThread(() => {
                UpdateFilteredContainers();
            });
        };

        // Monitor connection state changes
        this.dockerService.ConnectionStateChanged += (s, isConnected) => {
            MainThread.BeginInvokeOnMainThread(() => {
                this.ServiceState = isConnected ? "Connected" : "Disconnected";
            });
        };
    }

    public override void OnPageSizeChanged(double width, double height) {
        this.CollectionViewItemsLayoutSpan = (int)Math.Floor(width / 400);
    }

    [RelayCommand]
    private async Task Connect() {
        if (this.IsBusy) return;
        this.IsBusy = true;

        try {
            await this.dockerService.Connect();
            this.SystemInfo = this.dockerService.SystemInfo;
        } catch (Exception ex) {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        } finally {
            this.IsBusy = false;
        }
    }

    private void UpdateFilteredContainers() {
        var id = this.SelectedContainer?.Id;

        this.FilteredContainers.Clear();

        var filtered = dockerService.Container
            .Where(c => string.IsNullOrEmpty(SearchText) ||
                       c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       c.Image.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Name);

        foreach (var container in filtered) {
            this.FilteredContainers.Add(container);
        }

        if (id != null) {
            this.SelectedContainer = this.FilteredContainers.FirstOrDefault(c => c.Id == id);
        }
    }

    [RelayCommand]
    private async Task StartContainer(ContainerModel container) {
        if (this.IsBusy) return;
        this.IsBusy = true;

        try {
            await this.dockerService.StartContainersAsync(container.Id);
        } finally {
            this.IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StopContainer(ContainerModel container) {
        if (this.IsBusy) return;
        this.IsBusy = true;

        try {
            await this.dockerService.StopContainersAsync(container.Id);
        } finally {
            this.IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteContainer(ContainerModel container) {
        if (this.IsBusy) return;
        this.IsBusy = true;

        try {
            // Implement delete container
            //await dockerService.LoadContainersAsync();
            UpdateFilteredContainers();
        } finally {
            this.IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UpdateContainer(ContainerModel container) {
        if (this.IsBusy) return;
        this.IsBusy = true;

        try {
            await this.dockerService.UpdateContainersAsync(container.Id);
        } finally {
            this.IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToDetails(ContainerModel container) {
        if (this.IsBusy || container is null) return;
        this.IsBusy = true;

        try {
            //await Shell.Current.GoToAsync(nameof(ContainerDetailPage), true, new Dictionary<string, object> {
            //    { "Id", container.Id }
            //});

            await Shell.Current.GoToAsync($"{nameof(ContainerDetailPage)}?Id={container.Id}");
        } finally {
            this.IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value) {
        UpdateFilteredContainers();
    }
}
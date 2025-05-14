
namespace DockerDesktop.ViewModels;

[QueryProperty(nameof(Id), "Id")]
public partial class ContainerDetailViewModel : BaseViewModel {
    private readonly DockerService dockerService;

    [ObservableProperty]
    private string id = String.Empty;

    [ObservableProperty]
    private ContainerModel? container;

    public ContainerDetailViewModel(DockerService dockerService) {
        this.dockerService = dockerService;
    }

    public override void OnNavigatedTo(NavigatedToEventArgs args) {
        this.Container = this.dockerService.Container.Where(c => c.Id == this.Id).FirstOrDefault();

        if(this.Container is null) {
            Shell.Current.GoToAsync("..");
            return;
        }

        this.Title = this.Container.Name;
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
            //UpdateFilteredContainers();
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
}

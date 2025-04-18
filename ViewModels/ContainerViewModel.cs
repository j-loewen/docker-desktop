
using Docker.DotNet.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Timer = System.Timers.Timer;

namespace DockerDesktop.ViewModels;

public partial class ContainerViewModel : BaseViewModel
{
    private readonly DockerService dockerService;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private String serviceState = "Disconnected";

    public ObservableGroupedCollection<String, ContainerModel> FilteredContainers { get; } = [];

    [ObservableProperty]
    private ContainerModel? selectedContainer;


    public ContainerViewModel(DockerService dockerService)
    {
        this.Title = "Container";
        this.dockerService = dockerService;

        // Monitor container collection changes
        this.dockerService.Container.CollectionChanged += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateFilteredContainers();
            });
        };

        // Monitor connection state changes
        this.dockerService.ConnectionStateChanged += (s, isConnected) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                this.ServiceState = isConnected ? "Connected" : "Disconnected";
            });
        };
    }

    [RelayCommand]
    private async Task Connect()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            await dockerService.Connect();
            //updateTimer.Start(); // Start periodic updates after connection
            //UpdateFilteredContainers();
            //await UpdateMetrics();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectContainer(ContainerModel container)
    {
        if (container == null) return;

        this.SelectedContainer = container;
    }

    private void UpdateFilteredContainers()
    {
        FilteredContainers.Clear();

        var filtered = dockerService.Container
            .Where(c => string.IsNullOrEmpty(SearchText) ||
                       c.Names.Any(n => n.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                       c.Image.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var container in filtered)
        {
            FilteredContainers.AddItem(container.State, new ContainerModel(container));
        }
    }

    [RelayCommand]
    private async Task StartContainer(ContainerModel container)
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // Implement start container
            //await dockerService.LoadContainersAsync();
            //UpdateFilteredContainers();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteContainer(string containerId)
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // Implement delete container
            await dockerService.LoadContainersAsync();
            UpdateFilteredContainers();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        UpdateFilteredContainers();
    }
}
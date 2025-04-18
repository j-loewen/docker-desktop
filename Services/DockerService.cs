using Docker.DotNet;
using Docker.DotNet.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Diagnostics;

namespace DockerDesktop.Services;

public class DockerService : IDisposable
{
    private readonly SettingsService settingsService;
    private DockerClient? client;
    private readonly Dictionary<string, CancellationTokenSource> containerStatsCancellationTokens = new();
    private bool disposed;

    public event EventHandler<bool>? ConnectionStateChanged;

    public ObservableCollection<ContainerListResponse> Container { get; } = new();
    public ObservableCollection<ImagesListResponse> Images { get; } = new();
    public ObservableCollection<NetworkResponse> Networks { get; } = new();
    public ObservableCollection<VolumesListResponse> Volumes { get; } = new();
    public SystemInfoResponse SystemInfo { get; set; } = new();

    public DockerService(SettingsService settingsService)
    {
        this.settingsService = settingsService;
    }

    private void UpdateConnectionState(bool connected)
    {
        ConnectionStateChanged?.Invoke(this, connected);
    }

    public async Task Connect()
    {
        try
        {
            this.client = new DockerClientConfiguration(new Uri(this.settingsService.Settings.Host)).CreateClient();
            await this.LoadContainersAsync();
            UpdateConnectionState(true);
        }
        catch
        {
            UpdateConnectionState(false);
            throw;
        }
    }

    public async Task LoadContainersAsync()
    {
        if (this.client == null)
        {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        // Stop existing stats monitoring
        foreach (var cts in containerStatsCancellationTokens.Values)
        {
            cts.Cancel();
        }
        containerStatsCancellationTokens.Clear();

        IList<ContainerListResponse> containers = await this.client.Containers.ListContainersAsync(new ContainersListParameters()
        {
            All = true
        });

        // Start monitoring stats for running containers
        foreach (var container in containers.Where(c => c.State == "running"))
        {
            var cts = new CancellationTokenSource();
            containerStatsCancellationTokens[container.ID] = cts;

            //_ = MonitorContainerStatsAsync(container, cts.Token);
        }

        // Update container collection
        foreach (var container in containers)
        {
            var existingContainer = this.Container.FirstOrDefault(c => c.ID == container.ID);
            if (existingContainer != null)
            {
                var index = this.Container.IndexOf(existingContainer);
                this.Container[index] = container;
            }
            else
            {
                this.Container.Add(container);
            }
        }

        // Remove containers that no longer exist
        var containersToRemove = this.Container
            .Where(c => !containers.Any(nc => nc.ID == c.ID))
            .ToList();

        foreach (var container in containersToRemove)
        {
            this.Container.Remove(container);
        }
    }

    //private async Task MonitorContainerStatsAsync(ContainerListResponse container, CancellationToken cancellationToken) {
    //    try {
    //        var parameters = new ContainerStatsParameters {
    //            Stream = true // Enable streaming stats
    //        };

    //        using var stream = await this.client!.Containers.GetContainerStatsAsync(container.ID, parameters, cancellationToken);
    //        using var reader = new StreamReader(stream);

    //        while (!cancellationToken.IsCancellationRequested) {
    //            var statsJson = await reader.ReadLineAsync();
    //            if (string.IsNullOrEmpty(statsJson)) continue;

    //            try {
    //                var stats = JsonSerializer.Deserialize<ContainerStatsResponse>(statsJson);
    //                if (stats != null) {
    //                    UpdateContainerStats(container, stats);
    //                }
    //            } catch (JsonException ex) {
    //                Debug.WriteLine($"Error parsing container stats: {ex.Message}");
    //            }
    //        }
    //    } catch (OperationCanceledException) {
    //        // Normal cancellation, ignore
    //    } catch (Exception ex) {
    //        Debug.WriteLine($"Error monitoring container stats: {ex.Message}");
    //    }
    //}

    private void UpdateContainerStats(ContainerListResponse container, ContainerStatsResponse stats)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                //var existingContainer = Container.FirstOrDefault(c => c.ID == container.ID);
                //if (existingContainer != null) {
                //    // Calculate CPU percentage
                //    var cpuDelta = stats.CPUStats.CPUUsage.TotalUsage - stats.PreCPUStats.CPUUsage.TotalUsage;
                //    var systemDelta = stats.CPUStats.SystemUsage - stats.PreCPUStats.SystemUsage;

                //    if (systemDelta > 0.0) {
                //        //existingContainer.CPUPerc = $"{(cpuDelta / systemDelta * 100.0):F2}%";
                //    }

                //    // Update memory usage
                //    //existingContainer.SizeRootFs = stats.MemoryStats.Usage;

                //    // Force UI update
                //    var index = Container.IndexOf(existingContainer);
                //    Container[index] = existingContainer;
                //}
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating container stats: {ex.Message}");
        }
    }

    public async Task LoadImagesAsync()
    {
        if (this.client == null)
        {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        IList<ImagesListResponse> images = await this.client.Images.ListImagesAsync(new ImagesListParameters()
        {
            All = true
        });

        foreach (var image in images)
        {
            Boolean found = false;

            for (int i = 0; i < this.Images.Count; i++)
            {
                if (this.Images[i].ID == image.ID)
                {
                    this.Images[i] = image;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                this.Images.Add(image);
            }
        }

        var imagesToRemove = this.Images
            .Where(i => !images.Any(ni => ni.ID == i.ID))
            .ToList();

        foreach (var image in imagesToRemove)
        {
            this.Images.Remove(image);
        }
    }

    public async Task LoadNetworksAsync()
    {
        if (this.client == null)
        {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        IList<NetworkResponse> networks = await this.client.Networks.ListNetworksAsync(new NetworksListParameters());

        foreach (var network in networks)
        {
            Boolean found = false;

            for (int i = 0; i < this.Networks.Count; i++)
            {
                if (this.Networks[i].ID == network.ID)
                {
                    this.Networks[i] = network;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                this.Networks.Add(network);
            }
        }

        var networksToRemove = this.Networks
            .Where(n => !networks.Any(nn => nn.ID == n.ID))
            .ToList();

        foreach (var network in networksToRemove)
        {
            this.Networks.Remove(network);
        }
    }

    public async Task LoadVolumesAsync()
    {
        if (this.client == null)
        {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        var volumes = await this.client.Volumes.ListAsync();
        // TODO: Implement volume collection updates similar to other collections
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Cancel all stats monitoring
                foreach (var cts in containerStatsCancellationTokens.Values)
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                containerStatsCancellationTokens.Clear();

                client?.Dispose();
                UpdateConnectionState(false);
            }
            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

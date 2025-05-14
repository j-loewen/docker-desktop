using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace DockerDesktop.Services;

public class DockerService : IDisposable {
    private readonly SettingsService settingsService;
    private DockerClient? dockerClient;
    private CancellationTokenSource? cancellationToken;
    private Boolean disposed;

    public event EventHandler<Boolean>? ConnectionStateChanged;

    public ObservableCollection<ContainerModel> Container { get; } = new();
    public ObservableCollection<ImageModel> Images { get; } = new();
    public ObservableCollection<NetworkModel> Networks { get; } = new();
    public ObservableCollection<VolumeModel> Volumes { get; } = new();
    public SystemInfoModel SystemInfo { get; set; } = new();
    public Boolean IsConnected { get; set; } = false;

    public DockerService(SettingsService settingsService) {
        this.settingsService = settingsService;
    }

    private void UpdateConnectionState(Boolean connected) {
        this.IsConnected = connected;

        this.ConnectionStateChanged?.Invoke(this, connected);
    }

    public async Task Connect() {
        try {
            this.dockerClient = new DockerClientConfiguration(new Uri(this.settingsService.Settings.Host)).CreateClient();

            await this.LoadSystemInfoAsync();
            await this.LoadContainersAsync();
            await this.LoadImagesAsync();
            await this.LoadNetworksAsync();
            await this.LoadVolumesAsync();


            if (this.cancellationToken is not null) {
                this.cancellationToken.Cancel();
                this.cancellationToken.Dispose();
            }

            this.cancellationToken = new CancellationTokenSource();
            IProgress<Message> progress = new Progress<Message>(s => {
                //System.Diagnostics.Debug.WriteLine($"{s.Type}: {s.Action} - {s.Actor.Attributes["name"]}");

                if (s.Type.Equals("container", StringComparison.CurrentCultureIgnoreCase)) {
                    //s.ID
                    //s.Actor.Attributes["name"]
                    //s.Action
                    this.RefreshContainerAsync(s.ID).GetAwaiter();
                } else if (s.Type.Equals("image", StringComparison.CurrentCultureIgnoreCase)) {
                    var y = s;
                } else if (s.Type.Equals("network", StringComparison.CurrentCultureIgnoreCase)) {
                    //s.Actor.ID
                    //s.Actor.Attributes["name"]
                    //s.Actor.Attributes["container"]
                    //s.Action
                } else if (s.Type.Equals("volume", StringComparison.CurrentCultureIgnoreCase)) {
                    var y = s;
                }
            });

            _ = Task.Run(async () => await dockerClient.System.MonitorEventsAsync(new ContainerEventsParameters()
            {
                Filters = new Dictionary<string, IDictionary<string, bool>> {
                    {
                        "event", new Dictionary<string, bool> {
                            { "start", true },
                            { "stop", true },
                            { "destroy", true },
                            { "create", true },
                            { "pull", true },
                            { "connect", true },
                            { "disconnect", true },
                            { "die", true }
                        }
                    }, {
                        "type", new Dictionary<string, bool> {
                            { "container", true },
                            { "image", true },
                            { "network", true },
                            { "volume", true }
                        }
                    }
                }
            }, progress, cancellationToken.Token));

            UpdateConnectionState(true);
        } catch {
            UpdateConnectionState(false);
            throw;
        }
    }

    private async Task LoadSystemInfoAsync() {
        if (this.dockerClient is null) {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        SystemInfoResponse systemInfo = await this.dockerClient.System.GetSystemInfoAsync();

        this.SystemInfo = new SystemInfoModel() {
            Os = systemInfo.OperatingSystem,
            Architecture = systemInfo.Architecture,
            CpuCores = (int)systemInfo.NCPU,
            Memory = systemInfo.MemTotal,
            Name = systemInfo.Name,
            Containers = (int)systemInfo.Containers,
            ContainersRunning = (int)systemInfo.ContainersRunning,
            ContainersStopped = (int)systemInfo.ContainersStopped,
            ContainersPaused = (int)systemInfo.ContainersPaused,
            Images = (int)systemInfo.Images
        };
    }

    private async Task LoadContainersAsync() {
        if (this.dockerClient is null) {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        this.Container.Clear();

        IList<ContainerListResponse> containers = await this.dockerClient.Containers.ListContainersAsync(new ContainersListParameters() {
            All = true
        });

        foreach (var container in containers) {
            this.Container.Add(new ContainerModel() {
                Id = container.ID,
                Name = (container.Names.FirstOrDefault() ?? String.Empty).TrimStart('/'),
                State = container.State,
                Created = container.Created,
                Command = container.Command,
                Image = new ImageModel() {
                    Id = container.ImageID,
                    Name = container.Image
                },
                Status = container.Status,
                Network = new NetworkModel() {
                    Name = (container.NetworkSettings.Networks.FirstOrDefault().Key ?? String.Empty)
                }
            });

            var existing = this.Container.FirstOrDefault(c => c.Id == container.ID);
            if (existing is not null) {
                foreach (var label in container.Labels) {
                    existing.Labels.Add(new KeyValueModel() {
                        Key = label.Key,
                        Value = label.Value
                    });
                }
            }
        }
    }

    private async Task RefreshContainerAsync(String id) {
        if (this.dockerClient is null) {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        IList<ContainerListResponse> containers = await this.dockerClient.Containers.ListContainersAsync(new ContainersListParameters() {
            All = true,
            Filters = new Dictionary<string, IDictionary<string, bool>> {
                { "id", new Dictionary<string, bool> { { id, true } } }
            }
        });

        if (containers.Count == 0 && this.Container.FirstOrDefault(c => c.Id == id) is not null) {
            var existing = this.Container.FirstOrDefault(c => c.Id == id);

            if (existing is not null) {
                this.Container.Remove(existing);
            }
        } else if (containers.Count > 0) {
            foreach (var container in containers) {
                var existing = this.Container.FirstOrDefault(c => c.Id == container.ID);
                if (existing is not null) {
                    var idx = this.Container.IndexOf(existing);

                    this.Container[idx].Name = (container.Names.FirstOrDefault() ?? string.Empty).TrimStart('/');
                    this.Container[idx].State = container.State;
                    this.Container[idx].Created = container.Created;
                    this.Container[idx].Command = container.Command;
                    this.Container[idx].Image.Id = container.ImageID;
                    this.Container[idx].Image.Name = container.Image;
                    this.Container[idx].Status = container.Status;
                    this.Container[idx].Network.Name = (container.NetworkSettings.Networks.FirstOrDefault().Key ?? String.Empty);
                    this.Container[idx].Labels.Clear();
                    foreach (var label in container.Labels) {
                        this.Container[idx].Labels.Add(new KeyValueModel() {
                            Key = label.Key,
                            Value = label.Value
                        });
                    }
                } else {
                    this.Container.Add(new ContainerModel() {
                        Id = container.ID,
                        Name = (container.Names.FirstOrDefault() ?? String.Empty).TrimStart('/'),
                        State = container.State,
                        Created = container.Created,
                        Command = container.Command,
                        Image = new ImageModel() {
                            Id = container.ImageID,
                            Name = container.Image
                        },
                        Status = container.Status,
                        Network = new NetworkModel() {
                            Name = (container.NetworkSettings.Networks.FirstOrDefault().Key ?? String.Empty)
                        }
                    });

                    existing = this.Container.FirstOrDefault(c => c.Id == container.ID);
                    if (existing is not null) {
                        foreach (var label in container.Labels) {
                            existing.Labels.Add(new KeyValueModel() {
                                Key = label.Key,
                                Value = label.Value
                            });
                        }
                    }
                }
            }
        }
    }

    public async Task StartContainersAsync(String id) {
        if (this.dockerClient is null) {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        await this.dockerClient.Containers.StartContainerAsync(id, new ContainerStartParameters() { });
    }

    public async Task StopContainersAsync(String id) {
        if (this.dockerClient is null) {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        await this.dockerClient.Containers.StopContainerAsync(id, new ContainerStopParameters() { 
            WaitBeforeKillSeconds = 30
        });
    }

    public async Task UpdateContainersAsync(String id) {
        if (this.dockerClient is null) {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        await this.dockerClient.Containers.UpdateContainerAsync(id,  new ContainerUpdateParameters() {
            
        });
    }

    private async Task LoadImagesAsync() {
        if (this.dockerClient is null) {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        IList<ImagesListResponse> images = await this.dockerClient.Images.ListImagesAsync(new ImagesListParameters() {
            All = true
        });

        // Update existing containers or add new ones
        foreach (var image in images) {
            var existing = this.Images.FirstOrDefault(c => c.Id == image.ID);
            if (existing is not null) {
                var idx = this.Images.IndexOf(existing);

                this.Images[idx].Name = image.RepoTags.FirstOrDefault() ?? string.Empty;
                this.Images[idx].Size = image.Size;
                this.Images[idx].Created = image.Created;
            } else {
                this.Images.Add(new ImageModel() {
                    Id = image.ID,
                    Name = image.RepoTags.FirstOrDefault() ?? string.Empty,
                    Size = image.Size,
                    Created = image.Created
                });
            }

            var container = this.Container.FirstOrDefault(c => c.Image.Id == image.ID);
            if (container is not null) {
                container.Image.Name = image.RepoTags.FirstOrDefault() ?? string.Empty;
                container.Image.Size = image.Size;
                container.Image.Created = image.Created;
            }
        }

        // Remove containers that no longer exist
        var remove = this.Images
            .Where(c => !images.Any(nc => nc.ID == c.Id))
            .ToList();

        foreach (var image in remove) {
            this.Images.Remove(image);
        }
    }

    private async Task LoadNetworksAsync() {
        if (this.dockerClient is null) {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        IList<NetworkResponse> networks = await this.dockerClient.Networks.ListNetworksAsync(new NetworksListParameters());

        // Update existing containers or add new ones
        foreach (var network in networks) {
            var existing = this.Networks.FirstOrDefault(c => c.Id == network.ID);
            if (existing is not null) {
                var idx = this.Networks.IndexOf(existing);

                this.Networks[idx].Name = network.Name;
                this.Networks[idx].Driver = network.Driver;
            } else {
                this.Networks.Add(new NetworkModel() {
                    Id = network.ID,
                    Name = network.Name,
                    Driver = network.Driver
                });
            }

            var container = this.Container.FirstOrDefault(c => c.Network.Name == network.Name);
            if (container is not null) {
                container.Network.Id = network.ID;
                container.Network.Driver = network.Driver;
            }
        }

        // Remove containers that no longer exist
        var remove = this.Networks
            .Where(c => !networks.Any(nc => nc.ID == c.Id))
            .ToList();

        foreach (var network in remove) {
            this.Networks.Remove(network);
        }
    }

    private async Task LoadVolumesAsync() {
        if (this.dockerClient is null) {
            throw new InvalidOperationException("Docker client is not connected. Call Connect() first.");
        }

        VolumesListResponse volumes = await this.dockerClient.Volumes.ListAsync();

        // Update existing containers or add new ones
        foreach (var volume in volumes.Volumes) {
            var existing = this.Volumes.FirstOrDefault(c => c.Name == volume.Name);
            if (existing is not null) {
                var idx = this.Volumes.IndexOf(existing);

                this.Volumes[idx].Name = volume.Name;
                this.Volumes[idx].Driver = volume.Driver;
            } else {
                this.Volumes.Add(new VolumeModel() {
                    Name = volume.Name,
                    Driver = volume.Driver
                });
            }
        }

        // Remove containers that no longer exist
        var remove = this.Volumes
            .Where(c => !volumes.Volumes.Any(nc => nc.Name == c.Name))
            .ToList();

        foreach (var volume in remove) {
            this.Volumes.Remove(volume);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposed) {
            if (disposing) {
                if (this.cancellationToken is not null) {
                    this.cancellationToken.Cancel();
                    this.cancellationToken.Dispose();
                }

                if (this.dockerClient is not null) {
                    this.dockerClient.Dispose();
                    this.dockerClient = null;
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposed = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

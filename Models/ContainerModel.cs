using Docker.DotNet.Models;

namespace DockerDesktop.Models;

public partial class ContainerModel : ObservableObject
{

    public ContainerModel()
    {
        this.Image = new ImageModel();
    }

    public ContainerModel(ContainerListResponse container)
    {
        this.Id = container.ID;
        this.Name = (container.Names.FirstOrDefault() ?? string.Empty).TrimStart('/');
        this.State = container.State;
        this.Created = container.Created;
        this.Command = container.Command;
        this.Image = new ImageModel(container.ImageID, container.Image);
    }

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
}
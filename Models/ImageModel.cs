using Docker.DotNet.Models;

namespace DockerDesktop.Models;
public partial class ImageModel : ObservableObject
{

    public ImageModel(ImagesListResponse image)
    {
        this.Id = image.ID;
        this.Name = image.RepoTags.FirstOrDefault() ?? string.Empty;
    }

    public ImageModel(String id, String name)
    {
        this.Id = id;
        this.Name = name;
    }

    public ImageModel()
    {
    }

    [ObservableProperty]
    private String id = string.Empty;

    [ObservableProperty]
    private String name = string.Empty;
}

namespace DockerDesktop.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private Boolean isBusy = false;

    [ObservableProperty]
    String title = String.Empty;

    public bool IsNotBusy => !IsBusy;
}

namespace DockerDesktop.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private Boolean isBusy = false;

    [ObservableProperty]
    private String title = String.Empty;

    public Boolean IsNotBusy => !IsBusy;

    public virtual void OnPageSizeChanged(double width, double height) {
        // Handle page size changes if needed
    }

    public virtual void OnNavigatedTo(NavigatedToEventArgs args) {
        // Handle page size changes if needed
    }
}

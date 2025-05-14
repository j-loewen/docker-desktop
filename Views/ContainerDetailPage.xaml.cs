namespace DockerDesktop.Views;

public partial class ContainerDetailPage : ContentPage
{

    public ContainerDetailPage(ContainerDetailViewModel viewModel) {
        InitializeComponent();
        this.BindingContext = viewModel;
    }

    private void ContentPage_SizeChanged(object sender, EventArgs e) {
        ((BaseViewModel)this.BindingContext).OnPageSizeChanged(((ContentPage)sender).Width, ((ContentPage)sender).Height);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args) {
        base.OnNavigatedTo(args);

        ((BaseViewModel)this.BindingContext).OnNavigatedTo(args);
    }
}
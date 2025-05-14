namespace DockerDesktop.Views;

public partial class HomePage : ContentPage
{
	public HomePage(HomeViewModel viewModel)
	{
        InitializeComponent();
        this.BindingContext = viewModel;

        this.Title = "Dashboard";
    }

    private void ContentPage_SizeChanged(object sender, EventArgs e) {
        ((BaseViewModel)this.BindingContext).OnPageSizeChanged(((ContentPage)sender).Width, ((ContentPage)sender).Height);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args) {
        base.OnNavigatedTo(args);

        ((BaseViewModel)this.BindingContext).OnNavigatedTo(args);
    }
}
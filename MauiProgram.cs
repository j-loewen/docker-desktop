using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.Maui.Foldable;

namespace DockerDesktop
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder()
                            .UseMauiCommunityToolkit()
                            .UseFoldable()
                            .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("materialdesignicons.ttf", "MaterialDesignIcons"); //https://materialdesignicons.com/cdn/1.6.50-dev/
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
            builder.Services.AddSingleton<IMap>(Map.Default);

            //Services
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddSingleton<DockerService>();

            //ViewModels
            builder.Services.AddSingleton<ContainerPage, ContainerViewModel>();
            //builder.Services.AddSingleton<SettingsViewModel>();

            //Pages
            builder.Services.AddSingleton<SettingsPage, SettingsViewModel>();

            return builder.Build();
        }
    }
}

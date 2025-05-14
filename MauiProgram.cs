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

                    //https://fontawesome.com/cheatsheet
                    fonts.AddFont("fa-brands-400.ttf", "faBrands");
                    fonts.AddFont("fa-regular-400.ttf", "faRegular");
                    fonts.AddFont("fa-solid-900.ttf", "faSolid");
                    fonts.AddFont("fa-duotone-900.ttf", "faDuotone");
                    fonts.AddFont("fa-light-300.ttf", "faLight");
                    fonts.AddFont("fa-thin-100.ttf", "faThin");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            //builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            //builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
            //builder.Services.AddSingleton<IMap>(Map.Default);

            //Services
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddSingleton<DockerService>();

            //Views & ViewModels
            builder.Services.AddSingleton<HomePage, HomeViewModel>();
            builder.Services.AddSingleton<ContainerPage, ContainerViewModel>();
            builder.Services.AddSingleton<SettingsPage, SettingsViewModel>();
            builder.Services.AddTransient<ContainerDetailPage, ContainerDetailViewModel>();

            return builder.Build();
        }
    }
}

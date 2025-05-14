using System.Windows.Input;
using DockerDesktop.Models;
using DockerDesktop.Services;

namespace DockerDesktop.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly SettingsService settingsService;

    public SettingsViewModel(SettingsService settingsService)
    {
        this.settingsService = settingsService;
        this.settings = this.settingsService.Settings;
    }

    [ObservableProperty]
    private SettingsModel settings;

    [RelayCommand]
    public Task Save()
    {
        this.settingsService.Save();
        ThemeManager.SetTheme();

        return Task.CompletedTask;
    }
}

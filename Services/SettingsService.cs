namespace DockerDesktop.Services;

public class SettingsService
{

    public SettingsModel Settings { get; set; } = new();

    public SettingsService()
    {
        this.Load();
    }

    public void Save()
    {
        Preferences.Default.Set(nameof(SettingsModel.Host), this.Settings.Host);
        Preferences.Default.Set(nameof(SettingsModel.Theme), this.Settings.Theme);
    }

    private void Load()
    {
        this.Settings.Host = Preferences.Default.Get(nameof(SettingsModel.Host), String.Empty);
        this.Settings.Theme = Preferences.Default.Get(nameof(SettingsModel.Theme), 0);
    }
}
namespace DockerDesktop.Helpers;

public static class ThemeManager
{
    public static void SetTheme() {
        var theme = Preferences.Default.Get(nameof(SettingsModel.Theme), 0);

        if (theme == 0) {
            App.Current.UserAppTheme = AppTheme.Unspecified;
        } else if (theme == 1) {
            App.Current.UserAppTheme = AppTheme.Light;
        } else {
            App.Current.UserAppTheme = AppTheme.Dark;
        }
    }
}

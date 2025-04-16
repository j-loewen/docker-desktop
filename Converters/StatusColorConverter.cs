using System.Globalization;

namespace DockerDesktop.Converters;

public class StatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLower() == "running"
                ? Application.Current.Resources["StatusRunning"]
                : Application.Current.Resources["StatusStopped"];
        }
        return Application.Current.Resources["StatusStopped"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

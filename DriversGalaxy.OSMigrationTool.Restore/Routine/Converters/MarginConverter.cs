using System.Windows.Data;
using System.Windows;
using DriversGalaxy.OSMigrationTool.Restore.Infrastructure;

namespace DriversGalaxy.OSMigrationTool.Restore.Routine
{
    public sealed class MarginConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string tmpValue = WPFLocalizeExtensionHelpers.GetUIString(parameter.ToString());
            return new Thickness(System.Convert.ToDouble(tmpValue), 0, 0, 0);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}

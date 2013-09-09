using System;
using System.Windows.Data;
using System.Globalization;
using DriversGalaxy.Models;

namespace DriversGalaxy.Routine
{
    public sealed class PropertyEqualsToEnablingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            ScanStatus s1 = (ScanStatus)value;
            ScanStatus s2 = (ScanStatus)parameter;
            return s1 == s2 ? "True" : "False";
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

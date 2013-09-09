using System;
using System.Windows.Data;
using DriversGalaxy.Models;
using System.Globalization;

namespace DriversGalaxy.Routine
{
    public sealed class ScanStatusEqualsToEnablingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                      object parameter, CultureInfo culture)
        {
            string result = "False";
            ScanStatus s1 = (ScanStatus)value;
            if (s1 != ScanStatus.ScanStarted && s1 != ScanStatus.UpdateStarted)
                result = "True";
            return result;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

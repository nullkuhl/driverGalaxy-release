using System;
using System.Globalization;
using System.Windows.Data;
using DriversGalaxy.Models;
using System.Windows;

namespace DriversGalaxy.Routine
{
    public sealed class PaymentStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            ScanStatus s1 = (ScanStatus)value;
            if (s1 != ScanStatus.PaymentNeeded && s1 != ScanStatus.LicenseKeyEnter)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace DriversGalaxy.Routine
{
	public sealed class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
							  object parameter, CultureInfo culture)
		{
			bool s1 = (bool)value;
            if (parameter != null)
            {
                if (parameter is String && parameter.ToString() == "not")
                {
                    return s1 ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    bool s2 = bool.Parse(parameter.ToString());
                    return s1 == s2 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                return s1 ? Visibility.Visible : Visibility.Collapsed;
            }
		}

		public object ConvertBack(object value, Type targetType,
								  object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

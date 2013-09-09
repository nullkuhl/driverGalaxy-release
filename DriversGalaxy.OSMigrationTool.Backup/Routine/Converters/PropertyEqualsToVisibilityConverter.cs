using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DriversGalaxy.OSMigrationTool.Backup.Models;

namespace DriversGalaxy.OSMigrationTool.Backup.Routine
{
	public sealed class PropertyEqualsToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
							  object parameter, CultureInfo culture)
		{
			ScanStatus s1 = (ScanStatus)value;
			ScanStatus s2 = (ScanStatus)parameter;
			return s1 == s2 ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType,
								  object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

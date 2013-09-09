using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using DriversGalaxy.OSMigrationTool.Backup.Models;

namespace DriversGalaxy.OSMigrationTool.Backup.Routine
{
	public sealed class AnyPropertyEqualsToVisibilityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType,
							  object parameter, CultureInfo culture)
		{
			Visibility result = Visibility.Hidden;

			ScanStatus s1 = (ScanStatus)parameter;

			foreach (object val in values)
			{
				if (val != null && (ScanStatus)val == s1)
				{
					result = Visibility.Visible;
				}
			}

			if (values != null && values[0] != null && values[1] != null)
			{
				ScanStatus s2 = (ScanStatus)values[0];
				ScanStatus s3 = (ScanStatus)values[1];

				if (s2 == s3)
				{
					result = Visibility.Visible;
				}
			}

			return result;
		}

		public object[] ConvertBack(object values, Type[] targetTypes,
								  object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

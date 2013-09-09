using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using DriversGalaxy.OSMigrationTool.Restore.Models;

namespace DriversGalaxy.OSMigrationTool.Restore.Routine
{
	public sealed class AnyPropertyEqualsToVisibilityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType,
							  object parameter, CultureInfo culture)
		{
			Visibility result = Visibility.Hidden;

			InstallStatus s1 = (InstallStatus)parameter;

			foreach (object val in values)
			{
				if (val != null && (InstallStatus)val == s1)
				{
					result = Visibility.Visible;
				}
			}

			if (values != null && values[0] != null && values[1] != null)
			{
				InstallStatus s2 = (InstallStatus)values[0];
				InstallStatus s3 = (InstallStatus)values[1];

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

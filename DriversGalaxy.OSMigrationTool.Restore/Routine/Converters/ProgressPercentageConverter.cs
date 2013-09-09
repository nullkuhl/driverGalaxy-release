using System;
using System.Globalization;
using System.Windows.Data;

namespace DriversGalaxy.OSMigrationTool.Restore.Routine
{
	public sealed class ProgressPercentageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
							  object parameter, CultureInfo culture)
		{
			double v = int.Parse(value.ToString()) * 3.94;
			return v;
		}

		public object ConvertBack(object value, Type targetType,
								  object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

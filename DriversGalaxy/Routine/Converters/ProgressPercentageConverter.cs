using System;
using System.Globalization;
using System.Windows.Data;

namespace DriversGalaxy.Routine
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

    public sealed class FullBackupProgressPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            double v = int.Parse(value.ToString()) * 3.8;
            return v;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class BackupTargetsProgressPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            double v = int.Parse(value.ToString()) * 2.3;
            return v;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class RestoreTargetsProgressPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            double v = int.Parse(value.ToString()) * 1.9;
            return v;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

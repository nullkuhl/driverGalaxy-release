using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace DriversGalaxy.OSMigrationTool.Backup.Views
{
	/// <summary>
	/// Interaction logic for PanelScanFinished.xaml
	/// </summary>
	public partial class PanelScanFinished : UserControl
	{
		public PanelScanFinished()
		{
			InitializeComponent();
		}

		private void Click_Facebook(object sender, RoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo(@"http://www.freemium.com/fsu/facebook"));
		}

		private void Click_Twitter(object sender, RoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo(@"http://www.freemium.com/fsu/twitter"));
		}

		private void Click_GooglePlus(object sender, RoutedEventArgs e)
		{
			Process.Start(new ProcessStartInfo(@"http://www.freemium.com/fsu/googleplus"));
		}
	}
}
	
using System.Windows;
using System.Windows.Controls;

namespace DriversGalaxy.OSMigrationTool.Backup.Views
{
	/// <summary>
	/// Interaction logic for PanelScanInProcess.xaml
	/// </summary>
	public partial class PanelComposing : UserControl
	{
		public PanelComposing()
		{
			InitializeComponent();

			// Needed to activate Visibility changed trigger to start animation
			GreenProgressBar.Visibility = Visibility.Visible;
		}

		private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.ExtentHeightChange > 0.0)
				((ScrollViewer)e.OriginalSource).ScrollToEnd();
		}
	}
}

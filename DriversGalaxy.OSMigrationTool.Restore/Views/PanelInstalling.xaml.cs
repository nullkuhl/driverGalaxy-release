using System.Windows;
using System.Windows.Controls;

namespace DriversGalaxy.OSMigrationTool.Restore.Views
{
	/// <summary>
	/// Interaction logic for PanelInstalling.xaml
	/// </summary>
	public partial class PanelInstalling
	{
		public PanelInstalling()
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

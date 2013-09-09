using System.Windows;
using System.Windows.Controls;

namespace DriversGalaxy.Views
{
	/// <summary>
	/// Interaction logic for PanelScanInProcess.xaml
	/// </summary>
	public partial class PanelScanInProcess : UserControl
	{
		public PanelScanInProcess()
		{
			InitializeComponent();
		}

		private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.ExtentHeightChange > 0.0)
				((ScrollViewer)e.OriginalSource).ScrollToEnd();
		}
	}
}

using System.Windows;
using System.Windows.Controls;

namespace DriversGalaxy.Views
{
	/// <summary>
	/// Interaction logic for PanelScanUpdateInProcess.xaml
	/// </summary>
	public partial class PanelScanUpdateInProcess
	{
		public PanelScanUpdateInProcess()
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
	
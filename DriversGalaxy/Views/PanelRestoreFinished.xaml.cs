using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace DriversGalaxy.Views
{
	/// <summary>
	/// Interaction logic for PanelRestoreFinished.xaml
	/// </summary>
	public partial class PanelRestoreFinished : UserControl
	{
		public PanelRestoreFinished()
		{
			InitializeComponent();
		}

        private void RebootNow_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo cmdStartInfo = new ProcessStartInfo();
            cmdStartInfo.FileName = "cmd";
            cmdStartInfo.Arguments = "/c shutdown /f /r /t 0"; // Shutdown /force /restart /timeout=0
            cmdStartInfo.UseShellExecute = false; // Disable starting process from system shell
            cmdStartInfo.CreateNoWindow = true; // Don't create cmd window

            Process cmdProcess = new Process();
            cmdProcess.StartInfo = cmdStartInfo;
            cmdProcess.Start();
        }
	}
}
	
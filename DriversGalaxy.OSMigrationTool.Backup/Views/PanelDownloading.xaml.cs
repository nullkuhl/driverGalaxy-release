using System.Windows.Controls;
using System;
using Ookii.Dialogs.Wpf;
using DriversGalaxy.OSMigrationTool.Backup.Infrastructure;

namespace DriversGalaxy.OSMigrationTool.Backup.Views
{
	/// <summary>
	/// Interaction logic for PanelScan.xaml
	/// </summary>
	public partial class PanelDownloading : UserControl
	{
		public PanelDownloading()
		{
			InitializeComponent();
		}

		private void SelectDestinationDirectory_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
			dialog.Description = WPFLocalizeExtensionHelpers.GetUIString("SelectFolderForDownloads");
			dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
			if ((bool)dialog.ShowDialog())
			{
				if (!String.IsNullOrEmpty(dialog.SelectedPath))
				{
					DestinationDirectory.Text = dialog.SelectedPath;
				}
			}
		}

		private void DestinationDirectory_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			SelectDestinationDirectory_Click(null, null);
		}
	}
}

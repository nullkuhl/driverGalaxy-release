using System.Windows;
using System.Windows.Controls;

namespace DriversGalaxy.Views
{
	/// <summary>
	/// Interaction logic for PanelBackupAndRestore.xaml
	/// </summary>
	public partial class PanelBackupAndRestore : UserControl
	{
		public PanelBackupAndRestore()
		{
			InitializeComponent();
		}

		private void BackupActionChanged(object sender, RoutedEventArgs e)
		{
			if (BackupItemsList != null && BackupTypesList != null)
			{
				if (ActionBackup.IsChecked == true)
				{
					BackupItemsList.Visibility = Visibility.Collapsed;
					BackupTypesList.Visibility = Visibility.Visible;
					RestoreButton.Visibility = Visibility.Collapsed;
					BackupButton.Visibility = Visibility.Visible;
				}
				else
				{
					BackupTypesList.Visibility = Visibility.Collapsed;
					BackupItemsList.Visibility = Visibility.Visible;
					BackupButton.Visibility = Visibility.Collapsed;
					RestoreButton.Visibility = Visibility.Visible;
				}
			}
		}
	}
}
	
using System.Windows.Controls;
using System.Diagnostics;
using System;
using System.Windows;
using DriversGalaxy.OSMigrationTool.Restore.Infrastructure;

namespace DriversGalaxy.OSMigrationTool.Restore.Views
{
	/// <summary>
	/// Interaction logic for PanelInstallFinished.xaml
	/// </summary>
	public partial class PanelInstallFinished : UserControl
	{
		public PanelInstallFinished()
		{
			InitializeComponent();
		}

		private void Click_Facebook(object sender, System.Windows.RoutedEventArgs e)
		{
            OpenUrl(WPFLocalizeExtensionHelpers.GetUIString("FBUrl"), e);
		}

		private void Click_Twitter(object sender, System.Windows.RoutedEventArgs e)
		{
            OpenUrl(WPFLocalizeExtensionHelpers.GetUIString("TwitterUrl"), e);
		}

		private void Click_GooglePlus(object sender, System.Windows.RoutedEventArgs e)
		{
            OpenUrl(WPFLocalizeExtensionHelpers.GetUIString("GooglePlusUrl"), e);
		}

        /// <summary>
        /// Opens a specified URL with a system default browser
        /// </summary>
        /// <param name="url">URL to open</param>
        /// <param name="e"></param>
        public void OpenUrl(string url, RoutedEventArgs e)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("iexplore", "-new " + url);
                Process.Start(psi);
            }
            catch (Exception)
            {
                MessageBox.Show(WPFLocalizeExtensionHelpers.GetUIString("UrlCannotBeOpenedMessage") + Environment.NewLine + url,
                                WPFLocalizeExtensionHelpers.GetUIString("UrlCannotBeOpenedTitle"), MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            e.Handled = true;
        }
	}
}
	
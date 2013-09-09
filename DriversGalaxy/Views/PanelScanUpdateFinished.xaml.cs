using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using DriversGalaxy.Infrastructure;
using System;

namespace DriversGalaxy.Views
{
    /// <summary>
    /// Interaction logic for PanelScanUpdateFinished.xaml
    /// </summary>
    public partial class PanelScanUpdateFinished : UserControl
    {
        public PanelScanUpdateFinished()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opens the Freemium Utilities Facebook page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Click_Facebook(object sender, RoutedEventArgs e)
        {
            OpenUrl(WPFLocalizeExtensionHelpers.GetUIString("FBUrl"), e);
        }

        /// <summary>
        /// Opens the Freemium Utilities Twitter page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Click_Twitter(object sender, RoutedEventArgs e)
        {
            OpenUrl(WPFLocalizeExtensionHelpers.GetUIString("TwitterUrl"), e);
        }

        /// <summary>
        /// Opens the Freemium Utilities Google+ page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Click_GooglePlus(object sender, RoutedEventArgs e)
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

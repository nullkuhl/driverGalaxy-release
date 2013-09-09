using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using DriversGalaxy.Infrastructure;
using DriversGalaxy.ViewModels;
using Microsoft.Win32;
using FreemiumUtil;
using WPFLocalizeExtension.Engine;
using System.IO;
using Ookii.Dialogs.Wpf;

namespace DriversGalaxy.Views
{
    /// <summary>
    /// Interaction logic for PanelPreferences.xaml
    /// </summary>
    public partial class PanelPreferences
    {
        // The path to the key where Windows looks for startup applications
        readonly string registryKeyName = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\";
        const string registryAppName = "DriversGalaxy";
        readonly string appPath = Assembly.GetExecutingAssembly().Location;
        readonly string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
        readonly Dictionary<string, int> langIndex = new Dictionary<string, int> { { "en-US", 0 }, { "de-DE", 1 } };

        public PanelPreferences()
        {
            InitializeComponent();

            var lang = CfgFile.Get("Lang");
            if (!langIndex.ContainsKey(lang)) lang = "en-US";

            LanguagesList.SelectedIndex = langIndex[lang];
            MinToTray.IsChecked = CfgFile.Get("MinimizeToTray") == "1";
            object regValue = null;
            try
            {
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(registryKeyName))
                {
                    regValue = regKey.GetValue(registryAppName);
                }
            }
            catch
            {
            }
            StartUpAction.IsChecked = regValue != null;
            driverDownloadsFolder.Text = Uri.UnescapeDataString(CfgFile.Get("DriverDownloadsFolder"));
            backupsFolder.Text = Uri.UnescapeDataString(CfgFile.Get("BackupsFolder"));
        }

        private void selectDriverDownloadsFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
                            {
                                Description = WPFLocalizeExtensionHelpers.GetUIString("SelectFolderForDownloads"),
                                UseDescriptionForTitle = true
                            };
            // This applies to the Vista style dialog only, not the old dialog.
            var showDialog = dialog.ShowDialog();
            if (showDialog != null && (bool)showDialog)
            {
                if (!String.IsNullOrEmpty(dialog.SelectedPath))
                {
                    driverDownloadsFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void selectBackupsFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
                            {
                                Description = WPFLocalizeExtensionHelpers.GetUIString("SelectFolderForBackups"),
                                UseDescriptionForTitle = true
                            };
            // This applies to the Vista style dialog only, not the old dialog.
            var showDialog = dialog.ShowDialog();
            if (showDialog != null && (bool)showDialog)
            {
                if (!String.IsNullOrEmpty(dialog.SelectedPath))
                {
                    backupsFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void driverDownloadsFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(driverDownloadsFolder.Text))
            {
                CfgFile.Set("DriverDownloadsFolder", Uri.EscapeUriString(driverDownloadsFolder.Text));
            }
        }

        private void backupsFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(driverDownloadsFolder.Text))
            {
                CfgFile.Set("BackupsFolder", Uri.EscapeUriString(backupsFolder.Text));
            }
        }

        private void ShowScanPanel(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).ShowPanelScan(null, null);
        }

        private void MinToTrayOnClose(object sender, RoutedEventArgs e)
        {
            CfgFile.Set("MinimizeToTray", "1");
        }

        private void ShutdownOnClose(object sender, RoutedEventArgs e)
        {
            CfgFile.Set("MinimizeToTray", "0");
        }

        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguagesList.SelectedIndex == 0)
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
                CfgFile.Set("Lang", "en-US");
            }
            if (LanguagesList.SelectedIndex == 1)
            {
                LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("de-DE");
                CfgFile.Set("Lang", "de-DE");
            }

            var mainViewModel = (MainWindowViewModel)Application.Current.MainWindow.DataContext;
            if (mainViewModel != null)
            {
                mainViewModel.SetBackupTypes();
                mainViewModel.SetSocialButtonsMargin();
                mainViewModel.SetLabels();
            }
        }

        private void LaunchAtStartup(object sender, RoutedEventArgs e)
        {
            SetAutoRun(true);
        }

        private void DoNotLaunchAtStartup(object sender, RoutedEventArgs e)
        {
            SetAutoRun(false);
        }

        public void SetAutoRun(bool enable)
        {
            try
            {
                using (RegistryKey regKey = Registry.CurrentUser.CreateSubKey(registryKeyName))
                {
                    if (enable)
                        regKey.SetValue(registryAppName, appPath);
                    else
                        regKey.DeleteValue(registryAppName);
                }
            }
            catch
            {
            }
        }
    }
}

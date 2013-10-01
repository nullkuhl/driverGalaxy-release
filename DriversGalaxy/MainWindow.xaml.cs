using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using DriversGalaxy.Infrastructure;
using DriversGalaxy.Routine;
using DriversGalaxy.ViewModels;
using DriversGalaxy.Views;
using FreemiumUtilites;
using FreemiumUtil;
using DriversGalaxy.Models;
using WPFLocalizeExtension.Engine;
using System.Globalization;
using System.Threading;

namespace DriversGalaxy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>-
    public partial class MainWindow
    {
        Style navigationButtonStyle;
        Style navigationButtonSelectedStyle;
        Style navigationButtonFirstStyle;
        Style navigationButtonFirstSelectedStyle;

        public MainWindow()
        {
            InitializeComponent();

            string culture = CfgFile.Get("Lang");
            LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = LocalizeDictionary.Instance.Culture;

            using (var fs = new FileStream("Theme.xaml", FileMode.Open))
            {
                var dic = (ResourceDictionary)XamlReader.Load(fs);
                Application.Current.MainWindow.Resources.MergedDictionaries.Clear();
                Application.Current.MainWindow.Resources.MergedDictionaries.Add(dic);
            }

            // Wiring up View and ViewModel
            var model = new MainWindowViewModel();
            DataContext = model;

            navigationButtonStyle = (Style)FindResource("NavigationButton");
            navigationButtonSelectedStyle = (Style)FindResource("NavigationButton");
            navigationButtonFirstStyle = (Style)FindResource("NavigationButton");
            navigationButtonFirstSelectedStyle = (Style)FindResource("NavigationButton");

            ProcessFirstRun();
        }

        void ProcessFirstRun()
        {
            if (CfgFile.Get("FirstRun") == "1")
            {
                string appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Drivers Galaxy");
                Directory.CreateDirectory(appPath);
                string downloadsPath = Path.Combine(appPath, "Downloads");
                string backupsPath = Path.Combine(appPath, "Backups");
                Directory.CreateDirectory(downloadsPath);
                Directory.CreateDirectory(backupsPath);
                var panelPreferences = (PanelPreferences)Application.Current.MainWindow.FindName("PanelPreferences");
                panelPreferences.driverDownloadsFolder.Text = downloadsPath;
                var panelPreferences2 = (PanelPreferences2)Application.Current.MainWindow.FindName("PanelPreferences2");
                panelPreferences2.driverDownloadsFolder.Text = downloadsPath;
                CfgFile.Set("DriverDownloadsFolder", Uri.EscapeUriString(downloadsPath));
                panelPreferences.backupsFolder.Text = backupsPath;
                panelPreferences2.backupsFolder.Text = backupsPath;
                CfgFile.Set("BackupsFolder", Uri.EscapeUriString(backupsPath));
                CfgFile.Set("FirstRun", "0");
            }
        }

        public void LaunchPipe()
        {
            var mp = new messagePipe();
            mp.Activate();
            mp.Show();
            mp.Visible = false;

            if (App.startMinimized)
            {
                Hide();
                WindowState = WindowState.Minimized;
                ShowInTaskbar = false;
                double height = SystemParameters.WorkArea.Height;
                double width = SystemParameters.WorkArea.Width;
                Top = (height - Height) / 2;
                Left = (width - Width) / 2;
            }

        }

        #region Window operations

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            DragMove();
        }

        void CloseApp(object sender, RoutedEventArgs e)
        {
            if (CfgFile.Get("MinimizeToTray") == "1")
            {
                Hide();
            }
            else
            {
                //CfgFile.Set("MainWindowLeft", AppMainWindow.Left.ToString());
                //CfgFile.Set("MainWindowTop", AppMainWindow.Top.ToString());
                try
                {
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                }
                catch (Exception)
                {
                }
            }
        }

        void AppExit(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
            catch (Exception)
            {
            }
        }

        void MinimizeApp(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            if (CfgFile.Get("MinimizeToTray") == "1")
                Hide();
        }

        #endregion

        #region Panels animation

        bool AllAnimationsComplete()
        {
            return (PanelPreferences.Opacity == 0 || PanelPreferences.Opacity == 1) &&
                (PanelScanExclusions.Opacity == 0 || PanelScanExclusions.Opacity == 1) &&
                (PanelBackupAndRestore.Opacity == 0 || PanelBackupAndRestore.Opacity == 1) &&
                (PanelPreferences.Margin.Top == 0 || PanelPreferences.Margin.Top == -337) &&
                (PanelScanExclusions.Margin.Top == 0 || PanelScanExclusions.Margin.Top == -337) &&
                (PanelBackupAndRestore.Margin.Top == 0 || PanelBackupAndRestore.Margin.Top == -337) &&
                (PanelLicense.Margin.Top == 0 || PanelLicense.Margin.Top == -576) &&
                (PanelLicensePreferences.Margin.Top == 0 || PanelLicensePreferences.Margin.Top == -576);
        }

        void HideCurrentPanel()
        {
            var currentPanel = new FrameworkElement();
            if (PanelPreferences.IsVisible) { currentPanel = PanelPreferences; };
            if (PanelScan.IsVisible) { currentPanel = PanelScan; };
            if (PanelScanExclusions.IsVisible) { currentPanel = PanelScanExclusions; };
            if (PanelBackupAndRestore.IsVisible) { currentPanel = PanelBackupAndRestore; };            
            if (PanelLicense.IsVisible)
            {
                PanelScan.Visibility = Visibility.Hidden;
                currentPanel = PanelLicense;
            };
            if (PanelLicensePreferences.IsVisible) { currentPanel = PanelLicensePreferences; };
            currentPanel.Visibility = Visibility.Hidden;
        }

        void ShowPanelPreferences()
        {
            if (AllAnimationsComplete() && PanelPreferences.Visibility != Visibility.Visible)
            {
                HideCurrentPanel();
                PanelPreferences.Visibility = Visibility.Visible;

                PanelScanHeader.Visibility = Visibility.Hidden;
                PanelScanExclusionsHeader.Visibility = Visibility.Hidden;
                PanelBackupAndRestoreHeader.Visibility = Visibility.Hidden;
                PanelPreferencesHeader.Visibility = Visibility.Visible;

                LinkPanelScan.Style = navigationButtonFirstStyle;
                LinkPanelScanExclusions.Style = navigationButtonStyle;
                LinkPanelBackupAndRestore.Style = navigationButtonStyle;
            }
        }

        void ShowPanelPreferences2()
        {
            if (AllAnimationsComplete() && PanelLicensePreferences.Visibility != Visibility.Visible)
            {
                HideCurrentPanel();
                PanelLicensePreferences.Visibility = Visibility.Visible;

                PanelScanHeader.Visibility = Visibility.Hidden;
                PanelScanExclusionsHeader.Visibility = Visibility.Hidden;
                PanelBackupAndRestoreHeader.Visibility = Visibility.Hidden;
                PanelPreferencesHeader.Visibility = Visibility.Visible;
            }
        }

        public void ShowPanelScan(object sender, RoutedEventArgs e)
        {
            if (AllAnimationsComplete() && (PanelScan.Visibility != Visibility.Visible))
            {
                HideCurrentPanel();
                PanelScan.Visibility = Visibility.Visible;

                var scrollPanel = new ThicknessAnimation { Duration = TimeSpan.FromSeconds(0), To = new Thickness(0, 0, 0, 0) };
                PanelScan.BeginAnimation(MarginProperty, scrollPanel, HandoffBehavior.SnapshotAndReplace);

                PanelPreferencesHeader.Visibility = Visibility.Hidden;
                PanelScanExclusionsHeader.Visibility = Visibility.Hidden;
                PanelBackupAndRestoreHeader.Visibility = Visibility.Hidden;
                PanelScanHeader.Visibility = Visibility.Visible;

                LinkPanelScan.Style = navigationButtonFirstSelectedStyle;
                LinkPanelScanExclusions.Style = navigationButtonStyle;
                LinkPanelBackupAndRestore.Style = navigationButtonStyle;
            }
        }

        public void ShowPanelLicense(object sender, RoutedEventArgs e)
        {
            if (AllAnimationsComplete() && (PanelLicense.Visibility != Visibility.Visible))
            {
                HideCurrentPanel();
                PanelScan.Visibility = Visibility.Visible;
                PanelLicense.Visibility = Visibility.Visible;

                var scrollPanel = new ThicknessAnimation { Duration = TimeSpan.FromSeconds(0), To = new Thickness(0, 0, 0, 0) };
                PanelLicense.BeginAnimation(MarginProperty, scrollPanel, HandoffBehavior.SnapshotAndReplace);

                PanelPreferencesHeader.Visibility = Visibility.Hidden;
                PanelScanExclusionsHeader.Visibility = Visibility.Hidden;
                PanelBackupAndRestoreHeader.Visibility = Visibility.Hidden;
                PanelScanHeader.Visibility = Visibility.Visible;

                LinkPanelScan.Style = navigationButtonFirstSelectedStyle;
                LinkPanelScanExclusions.Style = navigationButtonStyle;
                LinkPanelBackupAndRestore.Style = navigationButtonStyle;
            }
        }

        void ShowPanelScanExclusions(object sender, RoutedEventArgs e)
        {
            if (AllAnimationsComplete() && PanelScanExclusions.Visibility != Visibility.Visible)
            {
                HideCurrentPanel();
                PanelScanExclusions.Visibility = Visibility.Visible;

                var scrollPanel = new ThicknessAnimation { Duration = TimeSpan.FromSeconds(0), To = new Thickness(0, 0, 0, 0) };
                PanelScanExclusions.BeginAnimation(MarginProperty, scrollPanel, HandoffBehavior.SnapshotAndReplace);

                PanelPreferencesHeader.Visibility = Visibility.Hidden;
                PanelScanHeader.Visibility = Visibility.Hidden;
                PanelBackupAndRestoreHeader.Visibility = Visibility.Hidden;
                PanelScanExclusionsHeader.Visibility = Visibility.Visible;

                LinkPanelScan.Style = navigationButtonFirstStyle;
                LinkPanelScanExclusions.Style = navigationButtonSelectedStyle;
                LinkPanelBackupAndRestore.Style = navigationButtonStyle;
            }
        }

        void ShowPanelBackupAndRestore(object sender, RoutedEventArgs e)
        {
            if (AllAnimationsComplete() && PanelBackupAndRestore.Visibility != Visibility.Visible)
            {
                HideCurrentPanel();
                PanelBackupAndRestore.Visibility = Visibility.Visible;

                //var scrollPanel = new ThicknessAnimation { Duration = TimeSpan.FromSeconds(0), To = new Thickness(0, 0, 0, 0) };
                //PanelBackupAndRestore.BeginAnimation(MarginProperty, scrollPanel, HandoffBehavior.SnapshotAndReplace);

                PanelPreferencesHeader.Visibility = Visibility.Hidden;
                PanelScanHeader.Visibility = Visibility.Hidden;
                PanelScanExclusionsHeader.Visibility = Visibility.Hidden;
                PanelBackupAndRestoreHeader.Visibility = Visibility.Visible;

                LinkPanelScan.Style = navigationButtonFirstStyle;
                LinkPanelScanExclusions.Style = navigationButtonStyle;
                LinkPanelBackupAndRestore.Style = navigationButtonSelectedStyle;
            }
        }

        void ElementLoaded(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(VisibilityAnimation.AnimationTypeProperty, VisibilityAnimation.AnimationType.Fade);
        }

        #endregion

        private void OptionChanged(object sender, SelectionChangedEventArgs e)
        {
            var context = (ComboBox)sender;
            if (context.SelectedIndex == 0)
            {
                if (PanelLicense.Visibility == Visibility.Visible)
                {
                    ShowPanelPreferences2();
                }
                else
                {
                    ShowPanelPreferences();
                }
            }
            if (context.SelectedIndex == 1)
            {
                OpenOSMigrationToolPopup();
            }
            if (context.SelectedIndex == 2)
            {
                Process.Start(new ProcessStartInfo(WPFLocalizeExtensionHelpers.GetUIString("FeedbackUrl")));
            }
            // This needed to save state unchecked
            context.SelectedIndex = -1;
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender != null)
                LaunchPipe();

            if (App.startMinimized)
            {
                Hide();
                WindowState = WindowState.Minimized;
                ShowInTaskbar = false;
                double height = SystemParameters.WorkArea.Height;
                double width = SystemParameters.WorkArea.Width;
                Top = (height - Height) / 2;
                Left = (width - Width) / 2;
            }
        }


        /// <summary>
        /// Opens an OSMigrationToolPopup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenOSMigrationToolPopup()
        {
            var osMigrationToolPopup = new OSMigrationToolPopup { Owner = this };

            osMigrationToolPopup.Left = Left + (Width / 2 - osMigrationToolPopup.Width / 2);
            var regToolsHeight = (int)osMigrationToolPopup.Height;
            osMigrationToolPopup.Height = 0;
            int topStart = (int)(Top + Height) + 30;
            osMigrationToolPopup.Top = topStart;
            var topFinal = (int)(Top + (Height / 2 - regToolsHeight / 2));

            const int fullAnimationDuration = 300;
            int heightAnimationDuration = (fullAnimationDuration * regToolsHeight / (topStart - topFinal));

            var slideUp = new DoubleAnimation
            {
                From = topStart,
                To = topFinal,
                Duration = new Duration(TimeSpan.FromMilliseconds(fullAnimationDuration))
            };
            osMigrationToolPopup.BeginAnimation(TopProperty, slideUp);

            var scaleUp = new DoubleAnimation
            {
                From = 0,
                To = regToolsHeight,
                Duration = new Duration(TimeSpan.FromMilliseconds(heightAnimationDuration))
            };
            osMigrationToolPopup.BeginAnimation(HeightProperty, scaleUp);

            osMigrationToolPopup.AnimateInnerBox();

            osMigrationToolPopup.ShowDialog();
        }

        /// <summary>
        /// Opens an AboutBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenAboutBox(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox { Owner = this };

            aboutBox.Left = Left + (Width / 2 - aboutBox.Width / 2);
            var regToolsHeight = (int)aboutBox.Height;
            aboutBox.Height = 0;
            int topStart = (int)(Top + Height) + 30;
            aboutBox.Top = topStart;
            var topFinal = (int)(Top + (Height / 2 - regToolsHeight / 2));

            const int fullAnimationDuration = 300;
            int heightAnimationDuration = (fullAnimationDuration * regToolsHeight / (topStart - topFinal));

            var slideUp = new DoubleAnimation
            {
                From = topStart,
                To = topFinal,
                Duration = new Duration(TimeSpan.FromMilliseconds(fullAnimationDuration))
            };
            aboutBox.BeginAnimation(TopProperty, slideUp);

            var scaleUp = new DoubleAnimation
            {
                From = 0,
                To = regToolsHeight,
                Duration = new Duration(TimeSpan.FromMilliseconds(heightAnimationDuration))
            };
            aboutBox.BeginAnimation(HeightProperty, scaleUp);

            aboutBox.AnimateInnerBox();

            aboutBox.ShowDialog();
        }

        #region URL links opening

        /// <summary>
        /// Opens a specified URL with a system default browser
        /// </summary>
        /// <param name="url">URL to open</param>
        /// <param name="e"></param>
        public void OpenUrl(string url, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(url));
            e.Handled = true;
        }

        /// <summary>
        /// Opens the root URL of the Freemium Utilites website
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenRootUrl(object sender, RoutedEventArgs e)
        {
            OpenUrl(WPFLocalizeExtensionHelpers.GetUIString("RootUrl"), e);
        }

        #endregion
    }
}

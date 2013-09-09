using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using DriversGalaxy.OSMigrationTool.Restore.ViewModels;
using DriversGalaxy.OSMigrationTool.Restore.Infrastructure;
using System.Diagnostics;
using System.Windows.Media.Animation;

namespace DriversGalaxy.OSMigrationTool.Restore
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			string path = String.Format(@"Themes/{0}/Theme.xaml", "Blue");
			using (var fs = new FileStream(path, FileMode.Open))
			{
				var dic = (ResourceDictionary)XamlReader.Load(fs);
				Application.Current.MainWindow.Resources.MergedDictionaries.Clear();
				Application.Current.MainWindow.Resources.MergedDictionaries.Add(dic);
			}
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

        /// <summary>
        /// Opens the root URL of the Freemium Utilites website
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenRootUrl(object sender, RoutedEventArgs e)
        {
            OpenUrl(WPFLocalizeExtensionHelpers.GetUIString("RootUrl"), e);
        }

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


		#region Window operations

		public void DragWindow(object sender, MouseButtonEventArgs args)
		{
			DragMove();
		}

		void CloseApp(object sender, RoutedEventArgs e)
		{
			//if (CfgFile.Get("MinimizeToTray") == "1")
			//{
			//    this.Hide();
			//}
			//else
			{
				Application.Current.Shutdown();
			}
		}

		//void AppExit(object sender, RoutedEventArgs e)
		//{
		//    Application.Current.Shutdown();
		//}

		void MinimizeApp(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
			//if (CfgFile.Get("MinimizeToTray") == "1")
				//this.Hide();
		}

		#endregion
	}
}

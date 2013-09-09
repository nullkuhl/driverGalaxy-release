using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Animation;
using System.Collections.Generic;

namespace DriversGalaxy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool startMinimized;
        public static bool Click1;
        System.Windows.Forms.NotifyIcon notifyIcon;

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (notifyIcon != null)
            {
                notifyIcon.Dispose();
            }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //Reducing the frames per second on the WPF animations update
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 10 }
                );

            //Adding resources
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.ToString());
            this.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("/DriversGalaxy.Infrastructure;component/CommonStyles/WindowStyles.xaml", UriKind.Relative)) as ResourceDictionary);
            this.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("/DriversGalaxy.Infrastructure;component/CommonStyles/ControlStyles.xaml", UriKind.Relative)) as ResourceDictionary);
            this.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("/DriversGalaxy.Infrastructure;component/CommonStyles/ScrollViewerStyles.xaml", UriKind.Relative)) as ResourceDictionary);
            this.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("/DriversGalaxy.Infrastructure;component/CommonStyles/ProgressBarStyles.xaml", UriKind.Relative)) as ResourceDictionary);
            this.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("/DriversGalaxy.Infrastructure;component/CommonStyles/SocialButtonStyles.xaml", UriKind.Relative)) as ResourceDictionary);
            this.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("/DriversGalaxy.Infrastructure;component/WPFMessageBox/Themes/Generic.xaml", UriKind.Relative)) as ResourceDictionary);

     
            startMinimized = false;
            Click1 = false;

            MainWindow window = new MainWindow();
            window.Show();
            notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = DriversGalaxy.Properties.Images.DriverGalaxy,
                Text = "DriversGalaxy",
                Visible = true
            };

            notifyIcon.DoubleClick +=
            delegate
            {
                MainWindow.Show();
                MainWindow.ShowInTaskbar = true;
                MainWindow.WindowState = WindowState.Normal;
                MainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            };
            notifyIcon.MouseUp += notifyIcon_MouseClick;
        }

        public void ProcessArguments(List<string> args)
        {
            for (int i = 0; i != args.Count; ++i)
            {
                if ((args[i] == "StartHidden") || (args[i] == "Click1"))
                {
                    startMinimized = true;
                }
            }
        }

        public void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var menu = (System.Windows.Controls.ContextMenu)MainWindow.FindResource("RightClickSystemTray");
                menu.IsOpen = true;
            }
        }

        
    }
}
using System.Linq;
using Microsoft.VisualBasic.ApplicationServices;

namespace DriversGalaxy
{
    class WindowsFormsApp : WindowsFormsApplicationBase
    {
        private App _wpfApp;

        public WindowsFormsApp()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
            _wpfApp = new App();
            _wpfApp.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            if (eventArgs.CommandLine.Count > 0)
            {
                _wpfApp.ProcessArguments(eventArgs.CommandLine.ToList());
            }
            if (_wpfApp.MainWindow.WindowState == System.Windows.WindowState.Minimized)
            {
                _wpfApp.MainWindow.WindowState = System.Windows.WindowState.Normal;
            }

            _wpfApp.MainWindow.Show();
            _wpfApp.MainWindow.ShowInTaskbar = true;
            _wpfApp.MainWindow.WindowState = System.Windows.WindowState.Normal;
            _wpfApp.MainWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }
    }
}

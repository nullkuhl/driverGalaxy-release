using System.Linq;
using System.Windows;
using DriversGalaxy.OSMigrationTool.Restore.ViewModels;
using System.Text;

namespace DriversGalaxy.OSMigrationTool.Restore
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			var window = new MainWindow();
			var viewModel = new MainWindowViewModel();
            //viewModel.ZipToUnpack = @"C:\Test\DriversGalaxy.OSMigrationTool.Restore\DriverData.zip";
            try
            {
                if (e.Args.Any())
                {
                    viewModel.ZipToUnpack = e.Args[0];
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //// Wiring up View and ViewModel
            window.DataContext = viewModel;

            viewModel.UnZipDriverData();
            viewModel.ReadDriverDataFromXML();

            window.Show();
		}
	}
}

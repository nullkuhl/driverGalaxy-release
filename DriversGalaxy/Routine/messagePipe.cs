using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows;
using DriversGalaxy.ViewModels;

namespace DriversGalaxy.Routine
{
	public partial class messagePipe : Form
	{
		private const int ScanMsg = 0xD100;
		private const int ScanUpdateMsg = 0xD111;
		#region Dll Imports

		[DllImport("user32")]
		private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

		[DllImport("user32")]
		private static extern int RegisterWindowMessage(string message);
		#endregion Dll Imports

		public messagePipe()
		{
			InitializeComponent();
		}

		private void messagePipe_Load(object sender, EventArgs e)
		{
			this.Visible = false;
		}

		protected override void WndProc(ref Message message)
        {
			if ((MainWindowViewModel)App.Current.MainWindow.DataContext != null &&
				((MainWindowViewModel)App.Current.MainWindow.DataContext).Status != Models.ScanStatus.ScanStarted &&
				((MainWindowViewModel)App.Current.MainWindow.DataContext).Status != Models.ScanStatus.UpdateStarted)
			{
				//filter the RF_TESTMESSAGE
				if ((message.Msg == ScanMsg))
				{
					((MainWindowViewModel)App.Current.MainWindow.DataContext).SelectAllAndScan(false);
				}
				else if ((message.Msg == ScanUpdateMsg))
				{
					((MainWindowViewModel)App.Current.MainWindow.DataContext).SelectAllAndScan(true);
				}
				//be sure to pass along all messages to the base also
				base.WndProc(ref message);
			}
        }
	}
}

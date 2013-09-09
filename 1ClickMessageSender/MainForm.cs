using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ClickMaint
{
	public class frmMain : Form
	{
		//private const int RF_TESTMESSAGE = 0xA123;
		private const int ScanMsg = 0xD100;
		private const int ScanUpdateMsg = 0xD111;

		[DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int SendMessage(IntPtr hwnd, [MarshalAs(UnmanagedType.U4)] int msg, IntPtr wParam, IntPtr lParam);

		public const int HWND_BROADCAST = 0xFFFF;

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32")]
		public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

		[DllImport("user32")]
		public static extern int RegisterWindowMessage(string message);

		public static readonly int WM_ACTIVATEAPP = RegisterWindowMessage("WM_MDIRESTORE");
		private ListBox lsbMessages;
		private GroupBox grbMain;
		private Label lblMessages;
		private Label lblProcID;

		public frmMain()
		{
			InitializeComponent();

			lblProcID.Text = string.Format("This process ID: {0}", Process.GetCurrentProcess().Id);
			try
			{
				//get this running process
				//Process proc = Process.GetCurrentProcess();
				//get all other (possible) running instances
				//   Process[] processes = Process.GetProcessesByName("DriverUtilities.vshost");

				Process[] processes = Process.GetProcessesByName("DriversGalaxy");

				string[] args = Environment.GetCommandLineArgs();

				
				if (processes.Length > 0)
				{
					SendMessage((IntPtr) HWND_BROADCAST, Array.Exists(args, s => s == "AutoUpdate") ? ScanUpdateMsg : ScanMsg,
					            IntPtr.Zero, IntPtr.Zero);
				}
				else
				{
					/*  MessageBox.Show("No other running applications found.");*/
					var p = new Process
					        	{
					        		StartInfo = {FileName = Environment.CurrentDirectory + "\\DriversGalaxy.exe", Arguments = "StartHidden"}
					        	};
					//MessageBox.Show(p.StartInfo.FileName);
					p.Start();
					Thread.Sleep(3000);

					SendMessage((IntPtr) HWND_BROADCAST, Array.Exists(args, s => s == "AutoUpdate") ? ScanUpdateMsg : ScanMsg,
					            IntPtr.Zero, IntPtr.Zero);
				}
				//var timer = new System.Windows.Forms.Timer();
		   //     timer.Tick += new EventHandler(timer_Tick); // Everytime timer ticks, timer_Tick will be called
		   //     timer.Interval = (1000) * (3);              // Timer will tick evert second
		   //     timer.Enabled = true;
		   //     timer.Start();
			}
			catch (Exception)
			{

			}
		}

	

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lsbMessages = new System.Windows.Forms.ListBox();
			this.grbMain = new System.Windows.Forms.GroupBox();
			this.lblMessages = new System.Windows.Forms.Label();
			this.lblProcID = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lsbMessages
			// 
			this.lsbMessages.Location = new System.Drawing.Point(8, 72);
			this.lsbMessages.Name = "lsbMessages";
			this.lsbMessages.Size = new System.Drawing.Size(280, 173);
			this.lsbMessages.TabIndex = 1;
			// 
			// grbMain
			// 
			this.grbMain.Location = new System.Drawing.Point(8, 40);
			this.grbMain.Name = "grbMain";
			this.grbMain.Size = new System.Drawing.Size(280, 2);
			this.grbMain.TabIndex = 2;
			this.grbMain.TabStop = false;
			// 
			// lblMessages
			// 
			this.lblMessages.Location = new System.Drawing.Point(8, 56);
			this.lblMessages.Name = "lblMessages";
			this.lblMessages.Size = new System.Drawing.Size(112, 16);
			this.lblMessages.TabIndex = 3;
			this.lblMessages.Text = "Received Messages:";
			// 
			// lblProcID
			// 
			this.lblProcID.Location = new System.Drawing.Point(104, 248);
			this.lblProcID.Name = "lblProcID";
			this.lblProcID.Size = new System.Drawing.Size(184, 16);
			this.lblProcID.TabIndex = 5;
			this.lblProcID.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// frmMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(296, 267);
			this.Controls.Add(this.lblProcID);
			this.Controls.Add(this.lblMessages);
			this.Controls.Add(this.grbMain);
			this.Controls.Add(this.lsbMessages);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmMain";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Communicating via Messages Demo";
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.ResumeLayout(false);

		}
		#endregion

		[MTAThread]
		static void Main() 
		{
		   
			
			//Application.Exit();
			new frmMain();
			Environment.Exit(0);
			//Application.EnableVisualStyles();
			//Application.Run(new Form1());
		}

		private void frmMain_Load(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		protected override void WndProc(ref Message message)
		{
			//filter the RF_TESTMESSAGE
			if (message.Msg == ScanMsg)
			{
				//display that we recieved the message, of course we could do
				//something else more important here.
				Environment.Exit(0);
			}
			//be sure to pass along all messages to the base also
			base.WndProc(ref message);
		}

		//void timer_Tick(object sender, EventArgs e)
		//{
		//    Environment.Exit(0);
		//}
	}
}

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Animation;

namespace FreemiumUtilites
{
	/// <summary>
	/// Interaction logic forDriverDetailInfoPopup.xaml
	/// </summary>
	public partial class DriverDetailInfoPopup
	{
        static DriverDetailInfoPopup clickContext;

		/// <summary>
        /// DriverDetailInfoPopup constructor
		/// </summary>
        public DriverDetailInfoPopup()
		{
			InitializeComponent();
            Loaded += DriverDetailInfoPopup_Loaded;
            Unloaded += DriverDetailInfoPopup_Unloaded;
			clickContext = this;
		}

		/// <summary>
		/// Animates the inner box
		/// </summary>
		public void AnimateInnerBox()
		{
			var animFadeIn = new DoubleAnimation {From = 0, To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(800))};
			Inner.BeginAnimation(OpacityProperty, animFadeIn);
		}

		/// <summary>
		/// Animates the OSMigrationToolPopup closing process
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Close(object sender, RoutedEventArgs e)
		{
			var sb = new Storyboard();

			var animFadeIn = new DoubleAnimation {From = 1, To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(300))};

			sb.Children.Add(animFadeIn);

			Storyboard.SetTarget(animFadeIn, this);
			Storyboard.SetTargetProperty(animFadeIn, new PropertyPath(OpacityProperty));
			sb.Completed += sb_Completed;
            
			sb.Begin();
		}

		/// <summary>
		/// Fired when the pre-closing animation is done.
		/// Actually closes the app.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void sb_Completed(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Checks is the mouse clicked inside the UI element
		/// </summary>
		/// <param name="element">UI element</param>
		/// <param name="point">Screen coordinates of the point where the mouse is clicked</param>
		/// <returns>True if mouse is clicked inside the UI element</returns>
		static bool IsMouseClickWithin(Window element, Point point)
		{
			return (point.X > element.Left && point.X < (element.Left + element.Width)) &&
			       (point.Y > element.Top && point.Y < (element.Top + element.Height));
		}

		/// <summary>
		/// Animates the window when the mouse is clicked outside it
		/// </summary>
		/// <param name="point">Screen coordinates of the point where the mouse is clicked</param>
		static void AnimateClickOutside(Point point)
		{
			if (clickContext != null && clickContext.IsActive && !IsMouseClickWithin(clickContext, point))
			{
				var sb = new Storyboard();

				var originalLeft = (int) clickContext.Left;

				var animLeft = new DoubleAnimation
				               	{
				               		From = originalLeft - 20,
				               		To = originalLeft + 20,
				               		Duration = new Duration(TimeSpan.FromMilliseconds(100)),
				               		AutoReverse = true,
				               		RepeatBehavior = new RepeatBehavior(2),
				               		FillBehavior = FillBehavior.Stop
				               	};

				sb.Children.Add(animLeft);

				Storyboard.SetTarget(animLeft, clickContext);
				Storyboard.SetTargetProperty(animLeft, new PropertyPath(LeftProperty));

				sb.Begin();
			}
		}

		/*
		 * Mouse hook
		 */

        public void DriverDetailInfoPopup_Loaded(object sender, RoutedEventArgs e)
		{
			hookID = SetHook(_proc);
		}

        void DriverDetailInfoPopup_Unloaded(object sender, RoutedEventArgs e)
		{
			UnhookWindowsHookEx(hookID);
		}

		#region Mouse hook

		const int WH_MOUSE_LL = 14;
		static readonly LowLevelMouseProc _proc = HookCallback;
		static IntPtr hookID = IntPtr.Zero;

		static IntPtr SetHook(LowLevelMouseProc proc)
		{
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule)
			{
				return SetWindowsHookEx(WH_MOUSE_LL, proc,
				                        GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0 && MouseMessages.WM_LBUTTONUP == (MouseMessages) wParam)
			{
				var hookStruct = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof (MSLLHOOKSTRUCT));
				//Debug.WriteLine(hookStruct.pt.x + ", " + hookStruct.pt.y);
				double x = Convert.ToDouble(hookStruct.pt.x);
				double y = Convert.ToDouble(hookStruct.pt.y);
				AnimateClickOutside(new Point(x, y));
			}
			return CallNextHookEx(hookID, nCode, wParam, lParam);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern IntPtr SetWindowsHookEx(int idHook,
		                                              LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
		                                            IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern IntPtr GetModuleHandle(string lpModuleName);

		#region Nested type: LowLevelMouseProc

		delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

		#endregion

		#region Nested type: MSLLHOOKSTRUCT

		[StructLayout(LayoutKind.Sequential)]
		struct MSLLHOOKSTRUCT
		{
			public POINT pt;
			readonly uint mouseData;
			public readonly uint flags;
			public readonly uint time;
			public readonly IntPtr dwExtraInfo;
		}

		#endregion

		#region Nested type: MouseMessages

		enum MouseMessages
		{
			WM_LBUTTONDOWN = 0x0201,
			WM_LBUTTONUP = 0x0202,
			WM_MOUSEMOVE = 0x0200,
			WM_MOUSEWHEEL = 0x020A,
			WM_RBUTTONDOWN = 0x0204,
			WM_RBUTTONUP = 0x0205
		}

		#endregion

		#region Nested type: POINT

		[StructLayout(LayoutKind.Sequential)]
		struct POINT
		{
			public readonly int x;
			public readonly int y;
		}

		#endregion

		#endregion
	}
}
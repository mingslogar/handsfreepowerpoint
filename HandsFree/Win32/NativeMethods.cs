using System;
using System.Runtime.InteropServices;

namespace HandsFree.Win32
{
	static class NativeMethods
	{
		[DllImport("User32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);
	}
}

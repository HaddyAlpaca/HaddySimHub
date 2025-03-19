using System.Runtime.InteropServices;

namespace Win32;

static class Messages
{
	public static IntPtr HWND_BROADCAST = new IntPtr(0xffff);

	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	public static extern int RegisterWindowMessage(string lpString);

	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	public static extern bool SendNotifyMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindow(string className, string windowName);

}

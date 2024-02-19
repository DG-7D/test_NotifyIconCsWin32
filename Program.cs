using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX;
using static Windows.Win32.UI.Shell.NOTIFY_ICON_DATA_FLAGS;
using static Windows.Win32.UI.Shell.NOTIFY_ICON_MESSAGE;
using static Windows.Win32.UI.Shell.SHSTOCKICONID;
using static Windows.Win32.UI.Shell.SHGSI_FLAGS;

class Program
{
    // CsWin32で定義してくれない？
    const int WM_USER = 0x0400;
    const int WM_LBUTTONDBLCLK = 0x0203;
    static void Main()
    {
        HWND hWnd;
        unsafe
        {
            hWnd = PInvoke.CreateWindowEx(0, "STATIC", "", 0, 0, 0, 0, 0, HWND.Null, null, null, null);
        }

        SHSTOCKICONINFO iconInfo = new() { cbSize = (uint)Marshal.SizeOf<SHSTOCKICONINFO>() };
        PInvoke.SHGetStockIconInfo(SIID_DELETE, SHGSI_ICON, ref iconInfo);

        NOTIFYICONDATAW notifyIconData;
        notifyIconData = new()
        {
            cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATAW)),
            hWnd = hWnd,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = WM_USER + 1,
            hIcon = iconInfo.hIcon,
            szTip = "test\nDouble Click to exit",
        };

        PInvoke.SetWindowLongPtr(hWnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate<WNDPROC>(
            (HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam) =>
            {
                if (uMsg == WM_USER + 1 && lParam == WM_LBUTTONDBLCLK)
                {
                    PInvoke.Shell_NotifyIcon(NIM_DELETE, in notifyIconData);
                    Environment.Exit(0);
                }
                return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
            }
        ));

        PInvoke.Shell_NotifyIcon(NIM_ADD, in notifyIconData);

        while (PInvoke.GetMessage(out var msg, HWND.Null, 0, 0) != 0)
        {
            PInvoke.TranslateMessage(in msg);
            PInvoke.DispatchMessage(in msg);
        }
        PInvoke.Shell_NotifyIcon(NIM_DELETE, in notifyIconData);
    }
}
using System;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Graphics.Dwm;
using static Windows.Win32.PInvoke;
using System.Runtime.CompilerServices;

namespace WinUI3CaptureSample
{
    static class WindowEnumerationHelper
    {
        public static bool IsWindowValidForCapture(HWND hwnd)
        {
            if (hwnd.Value.ToInt64() == 0)
            {
                return false;
            }

            if (hwnd == GetShellWindow())
            {
                return false;
            }

            if (!IsWindowVisible(hwnd))
            {
                return false;
            }

            if (GetAncestor(hwnd, GET_ANCESTOR_FLAGS.GA_ROOT) != hwnd)
            {
                return false;
            }

            var style = (WINDOW_STYLE)GetWindowLongPtr(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
            if (style.HasFlag(WINDOW_STYLE.WS_DISABLED))
            {
                return false;
            }

            uint cloaked = 0;
            var hrTemp = new HRESULT(0);
            unsafe
            {
                var cloakedPtr = Unsafe.AsPointer(ref cloaked);
                hrTemp = DwmGetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, cloakedPtr, (uint)Marshal.SizeOf(cloaked));
            }
            if (hrTemp != 0)
            {
                return false;
            }
            if (hrTemp == 0 && cloaked != 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsMinimized(HWND hwnd)
        {
            return IsIconic(hwnd).Value != 0;
        }
    }
}

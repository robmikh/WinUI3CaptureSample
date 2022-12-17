using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using static Windows.Win32.PInvoke;

namespace WinUI3CaptureSample
{
    class MonitorInfo
    {
        public bool IsPrimary { get; set; }
        public Vector2 ScreenSize { get; set; }
        public Rect MonitorArea { get; set; }
        public Rect WorkArea { get; set; }
        public string DeviceName { get; set; }
        public IntPtr Hmon { get; set; }
    }

    static class MonitorEnumerationHelper
    {
        public static IEnumerable<MonitorInfo> GetMonitors()
        {
            var result = new List<MonitorInfo>();

            unsafe
            {
                EnumDisplayMonitors(new EmptyHandle(), null,
                    delegate (HMONITOR hMonitor, HDC hdcMonitor, RECT* lprcMonitor, LPARAM dwData)
                    {
                        MONITORINFOEXW win32Info = new MONITORINFOEXW();
                        win32Info.monitorInfo.cbSize = (uint)Marshal.SizeOf(win32Info);
                        bool success = GetMonitorInfo(hMonitor, ref win32Info.monitorInfo);
                        if (success)
                        {
                            var info = new MonitorInfo
                            {
                                ScreenSize = new Vector2(win32Info.monitorInfo.rcMonitor.right - win32Info.monitorInfo.rcMonitor.left, win32Info.monitorInfo.rcMonitor.bottom - win32Info.monitorInfo.rcMonitor.top),
                                MonitorArea = new Rect(win32Info.monitorInfo.rcMonitor.left, win32Info.monitorInfo.rcMonitor.top, win32Info.monitorInfo.rcMonitor.right - win32Info.monitorInfo.rcMonitor.left, win32Info.monitorInfo.rcMonitor.bottom - win32Info.monitorInfo.rcMonitor.top),
                                WorkArea = new Rect(win32Info.monitorInfo.rcWork.left, win32Info.monitorInfo.rcWork.top, win32Info.monitorInfo.rcWork.right - win32Info.monitorInfo.rcWork.left, win32Info.monitorInfo.rcWork.bottom - win32Info.monitorInfo.rcWork.top),
                                IsPrimary = win32Info.monitorInfo.dwFlags > 0,
                                Hmon = hMonitor,
                                DeviceName = win32Info.szDevice.ToString()
                            };
                            result.Add(info);
                        }
                        return true;
                    }, IntPtr.Zero);
            }
            return result;
        }
    }
}

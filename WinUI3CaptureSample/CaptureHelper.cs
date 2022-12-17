using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using WinRT.Interop;
using Windows.Win32.System.WinRT.Graphics.Capture;

namespace WinUI3CaptureSample
{
    static class CaptureHelper
    {
        static readonly Guid GraphicsCaptureItemGuid = new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");

        public static GraphicsCaptureItem CreateItemForWindow(HWND hwnd)
        {
            GraphicsCaptureItem item = null;
            unsafe
            {
                item = CreateItemForCallback((IGraphicsCaptureItemInterop interop, Guid* guid) =>
                {
                    interop.CreateForWindow(hwnd, guid, out object raw);
                    return raw;
                });
            }
            return item;
        }

        public static GraphicsCaptureItem CreateItemForMonitor(HMONITOR hmon)
        {
            GraphicsCaptureItem item = null;
            unsafe
            {
                item = CreateItemForCallback((IGraphicsCaptureItemInterop interop, Guid* guid) =>
                {
                    interop.CreateForMonitor(hmon, guid, out object raw);
                    return raw;
                });
            }
            return item;
        }

        private unsafe delegate object InteropCallback(IGraphicsCaptureItemInterop interop, Guid* guid);

        private static GraphicsCaptureItem CreateItemForCallback(InteropCallback callback)
        {
            var interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
            GraphicsCaptureItem item = null;
            unsafe
            {
                var guid = GraphicsCaptureItemGuid;
                var guidPointer = (Guid*)Unsafe.AsPointer(ref guid);
                var raw = Marshal.GetIUnknownForObject(callback(interop, guidPointer));
                item = GraphicsCaptureItem.FromAbi(raw);
                Marshal.Release(raw);
            }
            return item;
        }
    }
}

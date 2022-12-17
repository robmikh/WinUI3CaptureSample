using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Win32.Graphics.Direct3D11;
using Windows.Win32.Graphics.Dxgi;
using WinRT;

namespace WinUI3CaptureSample
{
    static class CaptureSnapshot
    {
        public static async Task<IDirect3DSurface> CaptureAsync(ID3D11Device d3dDevice, GraphicsCaptureItem item)
        {
            var device = Direct3D11Helper.CreateDirect3DDeviceFromD3D11Device(d3dDevice);
            var framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                device,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                1,
                item.Size);
            var session = framePool.CreateCaptureSession(item);

            var taskCompletion = new TaskCompletionSource<Direct3D11CaptureFrame>();
            framePool.FrameArrived += (s, a) =>
            {
                var frame = s.TryGetNextFrame();
                taskCompletion.SetResult(frame);
            };
            session.StartCapture();

            var frame = await taskCompletion.Task;
            framePool.Dispose();
            session.Dispose();

            var surface = frame.Surface;
            return surface;
        }
    }
}

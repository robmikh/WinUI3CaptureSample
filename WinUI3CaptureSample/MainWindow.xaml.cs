using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Capture;
using Windows.Graphics.Imaging;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct3D11;
using Windows.Win32.Graphics.Gdi;
using WinRT.Interop;

namespace WinUI3CaptureSample
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            Title = "WinUI3CaptureSample";

            _hwnd = new HWND(WindowNative.GetWindowHandle(this));
            _d3dDevice = Direct3D11Helper.CreateD3DDevice();

            InitWindowList();
            InitMonitorList();
        }

        private async void PickerButton_Click(object sender, RoutedEventArgs e)
        {
            StopCapture();
            WindowComboBox.SelectedIndex = -1;
            MonitorComboBox.SelectedIndex = -1;
            await StartPickerCaptureAsync();
        }

        private void PrimaryMonitorButton_Click(object sender, RoutedEventArgs e)
        {
            StopCapture();
            WindowComboBox.SelectedIndex = -1;
            MonitorComboBox.SelectedIndex = -1;
            StartPrimaryMonitorCapture();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            StopCapture();
            WindowComboBox.SelectedIndex = -1;
            MonitorComboBox.SelectedIndex = -1;
        }

        private void WindowComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var process = (Process)comboBox.SelectedItem;

            if (process != null)
            {
                StopCapture();
                MonitorComboBox.SelectedIndex = -1;
                var hwnd = (HWND)process.MainWindowHandle;
                try
                {
                    StartHwndCapture(hwnd);
                }
                catch (Exception)
                {
                    Debug.WriteLine($"Hwnd 0x{hwnd.Value:X8} is not valid for capture!");
                    _processes.Remove(process);
                    comboBox.SelectedIndex = -1;
                }
            }
        }

        private void MonitorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var monitor = (MonitorInfo)comboBox.SelectedItem;

            if (monitor != null)
            {
                StopCapture();
                WindowComboBox.SelectedIndex = -1;
                var hmon = (HMONITOR)monitor.Hmon;
                try
                {
                    StartHmonCapture(hmon);
                }
                catch (Exception)
                {
                    Debug.WriteLine($"Hmon 0x{hmon.Value:X8} is not valid for capture!");
                    _monitors.Remove(monitor);
                    comboBox.SelectedIndex = -1;
                }
            }
        }

        private void InitWindowList()
        {
            if (ApiInformation.IsApiContractPresent(typeof(UniversalApiContract).FullName, 8))
            {
                var processesWithWindows = from p in Process.GetProcesses()
                                           where !string.IsNullOrWhiteSpace(p.MainWindowTitle) && ShouldIncludeWindow(new HWND(p.MainWindowHandle))
                                           select p;
                _processes = new ObservableCollection<Process>(processesWithWindows);
                WindowComboBox.ItemsSource = _processes;
            }
            else
            {
                WindowComboBox.IsEnabled = false;
            }
        }

        private bool ShouldIncludeWindow(HWND hwnd)
        {
            return WindowEnumerationHelper.IsWindowValidForCapture(hwnd) && !WindowEnumerationHelper.IsMinimized(hwnd);
        }

        private void InitMonitorList()
        {
            if (ApiInformation.IsApiContractPresent(typeof(UniversalApiContract).FullName, 8))
            {
                var monitors = MonitorEnumerationHelper.GetMonitors();
                _monitors = new ObservableCollection<MonitorInfo>(monitors);
                MonitorComboBox.ItemsSource = _monitors;
            }
            else
            {
                MonitorComboBox.IsEnabled = false;
                PrimaryMonitorButton.IsEnabled = false;
            }
        }

        private async Task StartPickerCaptureAsync()
        {
            var picker = new GraphicsCapturePicker();
            InitializeWithWindow.Initialize(picker, _hwnd);
            var item = await picker.PickSingleItemAsync();

            if (item != null)
            {
                StartCaptureFromItem(item);
            }
        }

        private void StartHwndCapture(HWND hwnd)
        {
            var item = CaptureHelper.CreateItemForWindow(hwnd);
            if (item != null)
            {
                StartCaptureFromItem(item);
            }
        }

        private void StartHmonCapture(HMONITOR hmon)
        {
            var item = CaptureHelper.CreateItemForMonitor(hmon);
            if (item != null)
            {
                StartCaptureFromItem(item);
            }
        }

        private void StartPrimaryMonitorCapture()
        {
            var monitor = (from m in MonitorEnumerationHelper.GetMonitors()
                           where m.IsPrimary
                           select m).First();
            StartHmonCapture((HMONITOR)monitor.Hmon);
        }

        private void StopCapture()
        {
            ScreenshotImage.Source = null;
        }

        private async void StartCaptureFromItem(GraphicsCaptureItem item)
        {
            var surface = await CaptureSnapshot.CaptureAsync(_d3dDevice, item);
            var softwareBitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(surface, BitmapAlphaMode.Premultiplied);

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap);

            ScreenshotImage.Source = source;
        }

        private HWND _hwnd;
        private ObservableCollection<Process> _processes;
        private ObservableCollection<MonitorInfo> _monitors;

        private ID3D11Device _d3dDevice;
    }
}

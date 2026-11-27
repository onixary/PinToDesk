using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace PinToDesk.Common
{
    public partial class AboutWindow : Window
    {
        private readonly GlobalMouseClickMonitor _mouseMonitor = new();

        public AboutWindow()
        {
            InitializeComponent();
            InitVersion();

            _mouseMonitor.ClickedOutside += () => Dispatcher.BeginInvoke(Close);

            Loaded += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    _mouseMonitor.Start(new[] { hwnd });
                }
            };

            Closed += (s, e) => _mouseMonitor.Dispose();
        }

        private void InitVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                string vStr = (version.Build > 0) ? version.ToString(3) : $"{version.Major}.{version.Minor}";
                VersionText.Text = $"v{vStr}";
            }
            else
            {
                VersionText.Text = "未知";
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Hyperlink or FrameworkContentElement { Parent: Hyperlink }) return;
            if (e.LeftButton == MouseButtonState.Pressed) NativeMethods.DragMoveWithBounds(this);
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void OpenRepository(object sender, RequestNavigateEventArgs e)
        {
            if (sender != OriginalRepository && sender != EditedRepository) return;
            e.Handled = true;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = ((Hyperlink)sender).NavigateUri.AbsoluteUri,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"无法打开链接: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

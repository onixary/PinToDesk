using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PinToDesk.Common
{
    /// <summary>
    /// 轻量级全局鼠标低级钩子：在激活期间监听任意鼠标按键按下事件，判断是否在指定窗口外部
    /// </summary>
    public class GlobalMouseClickMonitor : IDisposable
    {
        private NativeMethods.LowLevelProc? _proc;
        private IntPtr _hookID = IntPtr.Zero;

        private readonly HashSet<IntPtr> _protectedHwnds = new();

        /// <summary>点击发生在保护窗口外部时触发</summary>
        public event Action? ClickedOutside;

        public void Start(IEnumerable<IntPtr> protectedHwnds)
        {
            _protectedHwnds.Clear();
            foreach (var h in protectedHwnds) _protectedHwnds.Add(h);

            if (_hookID != IntPtr.Zero) return;

            _proc = MouseHookCallback;
            using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            if (curModule?.ModuleName != null)
            {
                IntPtr hMod = NativeMethods.GetModuleHandle(curModule.ModuleName);
                _hookID = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _proc, hMod, 0);
                if (_hookID == IntPtr.Zero)
                {
                    System.Threading.Thread.Sleep(30);
                    _hookID = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _proc, hMod, 0);
                }
            }
        }

        public void Stop()
        {
            if (_hookID != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                if (msg == NativeMethods.WM_LBUTTONDOWN || msg == NativeMethods.WM_RBUTTONDOWN ||
                    msg == NativeMethods.WM_MBUTTONDOWN)
                {
                    var info = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
                    var hwndClicked = NativeMethods.WindowFromPoint(info.pt);
                    var rootClicked = NativeMethods.GetAncestor(hwndClicked, NativeMethods.GA_ROOT);

                    bool isProtected = _protectedHwnds.Contains(hwndClicked)
                                    || _protectedHwnds.Contains(rootClicked);

                    if (!isProtected)
                    {
                        ClickedOutside?.Invoke();
                    }
                }
            }
            return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose() => Stop();
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NInspect
{
    internal static class MouseHookManager
    {
        // WH_MOUSE_LL: https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-mouse-hook-in-c/
        // good answers on keyboard hooks: https://stackoverflow.com/questions/604410/global-keyboard-capture-in-c-sharp-application
        // very messy, only works in principle: https://support.microsoft.com/en-us/help/318804/how-to-set-a-windows-hook-in-visual-c-net
        // references: https://msdn.microsoft.com/en-us/library/windows/desktop/ms644986(v=vs.85).aspx

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseLowLevelHookStruct
        {
            public Point Point;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        private const int WH_MOUSE_LL = 0xE;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }
        private static IntPtr hHook = IntPtr.Zero;

        private static LowLevelMouseProc MouseProc = HookCallback;

        public static event MouseEventHandler MouseDown;
        public static event MouseEventHandler MouseUp;

        private static List<IntPtr> ignoreHandles = new List<IntPtr>();

        public static void IgnoreInsideWindowHandle(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                throw new ArgumentException();

            ignoreHandles.Add(hWnd);
        }

        public static void SetHook()
        {
            if (hHook != IntPtr.Zero)
                throw new InvalidOperationException();

            hHook = SetWindowsHookEx(MouseProc);

            if (hHook == IntPtr.Zero)
            {
                // TODO: Failed to create hook
            }
        }

        public static bool Unhook()
        {
            MouseDown = null;
            MouseUp = null;

            return User32.UnhookWindowsHookEx(hHook);
        }

        private static IntPtr SetWindowsHookEx(LowLevelMouseProc hProc)
        {
            using (var process = Process.GetCurrentProcess())
            using (var mainModule = process.MainModule)
            {
                IntPtr hModule = Kernel32.GetModuleHandle(mainModule.ModuleName);

                return User32.SetWindowsHookEx(WH_MOUSE_LL, hProc, hModule, 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var mouseInfo = (MouseLowLevelHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLowLevelHookStruct));

                OnMouseDown((MouseMessages)wParam, mouseInfo);
            }

            // Always call CallNextHookEx to let other hooks handle the event

            return User32.CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        private static void OnMouseDown(MouseMessages message, MouseLowLevelHookStruct mouseInfo)
        {
            var hWnd = User32.WindowFromPoint(mouseInfo.Point);

            var control = Control.FromHandle(hWnd);

            while (control != null)
            {
                if (ignoreHandles.Contains(control.Handle))
                {
                    // Ignore this message
                    return;
                }
                control = control.Parent;
            }

            MouseButtons button;

            if (message == MouseMessages.WM_LBUTTONDOWN ||
                message == MouseMessages.WM_LBUTTONUP)
            {
                button = MouseButtons.Left;
            }
            else if (message == MouseMessages.WM_RBUTTONDOWN ||
                message == MouseMessages.WM_RBUTTONUP)
            {
                button = MouseButtons.Right;
            }
            else
            {
                // TODO: other events?
                return;
            }

            var mouseEventArgs = new MouseEventArgs(
                button: button,
                clicks: 1,
                x: mouseInfo.Point.X,
                y: mouseInfo.Point.Y,
                delta: 0);

            if (message == MouseMessages.WM_LBUTTONDOWN ||
                message == MouseMessages.WM_RBUTTONDOWN)
            {
                MouseDown?.Invoke(null, mouseEventArgs);
            }
            else if (message == MouseMessages.WM_LBUTTONUP ||
                message == MouseMessages.WM_RBUTTONUP)
            {
                MouseUp?.Invoke(null, mouseEventArgs);
            }
        }
    }
}

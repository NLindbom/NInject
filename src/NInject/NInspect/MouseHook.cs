using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NInspect
{
    public class MouseHook
    {
        // https://stackoverflow.com/questions/604410/global-keyboard-capture-in-c-sharp-application
        // https://support.microsoft.com/en-us/help/318804/how-to-set-a-windows-hook-in-visual-c-net

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseHookInfo
        {
            public Point point;
            public int hWnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        private IntPtr hHook = IntPtr.Zero;

        public MouseHook()
        {
        }

        public IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var mouseHookInfo = (MouseHookInfo)Marshal.PtrToStructure(lParam, typeof(MouseHookInfo));

            if (nCode < 0)
            {
                //return User32.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            else
            {

                //return User32.CallNextHookEx(hHook, nCode, wParam, lParam);
            }

            throw new NotImplementedException();
        }
    }
}

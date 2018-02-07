using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NInject
{
    public static class ProcessManager
    {
        // http://www.aboutmycode.com/net-framework/how-to-get-elevated-process-path-in-net/

        public static string GetExecutablePath(Process Process)
        {
            // If running on Vista or later use the new function
            if (Environment.OSVersion.Version.Major >= 6)
            {
                return GetExecutablePathAboveVista(Process.Id);
            }

            return Process.MainModule.FileName;
        }

        private static string GetExecutablePathAboveVista(int ProcessId)
        {
            var buffer = new StringBuilder(1024);
            IntPtr hProcess = Kernel32.OpenProcess(Kernel32.ProcessAccessFlags.QueryLimitedInformation, false, ProcessId);

            if (hProcess != IntPtr.Zero)
            {
                try
                {
                    int size = buffer.Capacity;
                    if (Kernel32.QueryFullProcessImageName(hProcess, 0, buffer, out size))
                    {
                        return buffer.ToString();
                    }
                }
                finally
                {
                    Kernel32.CloseHandle(hProcess);
                }
            }

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static NInject.Kernel32.ProcessAccessFlags;
using static NInject.Kernel32.AllocationType;

namespace NInject
{
    public static class ProcessManager
    {
        private const uint STILL_ACTIVE = 259;

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
            IntPtr hProcess = Kernel32.OpenProcess(QueryLimitedInformation, false, ProcessId);

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

        // https://social.msdn.microsoft.com/Forums/en-US/6ddd7a04-3052-4080-9b77-155cf6d68828/calling-a-function-from-a-dll-injected-into-a-remote-process?forum=vcgeneral

        /// <returns>Remote thread exit code</returns>
        public static async Task<uint> RemoteLibaryFunction(Process targetProcess,
            string lpModuleName,
            string lpProcName,
            byte[] lpParameters,
            uint dwParamSize)
        {
            IntPtr hProcess = IntPtr.Zero;

            try
            {
                hProcess = Kernel32.OpenProcess(
                    CreateThread | QueryInformation | VirtualMemoryOperation | VirtualMemoryWrite | VirtualMemoryRead,
                    false,
                    targetProcess.Id);

                if (hProcess == IntPtr.Zero)
                {
                    throw new ProcessException(ProcessException.Reasons.OpenProcessFault, targetProcess, lpModuleName, lpProcName);
                }

                IntPtr lpRemoteParams;

                IntPtr hModule = Kernel32.GetModuleHandle(lpModuleName);

                IntPtr lpFunctionAddress = Kernel32.GetProcAddress(hModule, lpProcName);

                if (lpFunctionAddress == IntPtr.Zero)
                {
                    hModule = Kernel32.LoadLibrary(lpModuleName);

                    lpFunctionAddress = Kernel32.GetProcAddress(hModule, lpProcName);
                }

                if (lpFunctionAddress == IntPtr.Zero)
                {
                    throw new ProcessException(ProcessException.Reasons.InvalidFunctionAdress, targetProcess, lpModuleName, lpProcName);
                }

                if (lpParameters != null)
                {
                    IntPtr lpAddress = IntPtr.Zero;

                    lpRemoteParams = Kernel32.VirtualAllocEx(hProcess,
                        lpAddress,
                        dwParamSize,
                        MemReserve | MemCommit,
                        Kernel32.Protect.PageExecuteReadWrite);

                    if (lpRemoteParams == IntPtr.Zero)
                    {
                        throw new ProcessException(ProcessException.Reasons.RemoteParameterAllocationFault, targetProcess, lpModuleName, lpProcName);
                    }

                    UIntPtr dwBytesWritten;

                    bool result = Kernel32.WriteProcessMemory(hProcess,
                        lpRemoteParams,
                        lpParameters,
                        dwParamSize,
                        out dwBytesWritten);

                    if (!result || dwBytesWritten == UIntPtr.Zero)
                    {
                        throw new ProcessException(ProcessException.Reasons.WriteProcessMemoryFault, targetProcess, lpModuleName, lpProcName);
                    }
                }
                else
                {
                    lpRemoteParams = IntPtr.Zero;
                }

                uint dwCreationFlags = 0;
                IntPtr lpThreadedId = IntPtr.Zero;
                uint dwStackSize = 0;
                IntPtr lpThreadAttributes = IntPtr.Zero;

                IntPtr hThread = Kernel32.CreateRemoteThread(hProcess,
                    lpThreadAttributes,
                    dwStackSize,
                    lpFunctionAddress,
                    lpRemoteParams,
                    dwCreationFlags,
                    lpThreadedId);

                if (hThread == IntPtr.Zero)
                {
                    throw new ProcessException(ProcessException.Reasons.CreateRemoteThreadFault, targetProcess, lpModuleName, lpProcName);
                }

                return await GetExitCodeThread(hThread);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                Kernel32.CloseHandle(hProcess);
            }
        }

        private static async Task<uint> GetExitCodeThread(IntPtr hThread)
        {
            uint dwOut;

            while (Kernel32.GetExitCodeThread(hThread, out dwOut))
            {
                if (dwOut != STILL_ACTIVE)
                {
                    return dwOut;
                }

                await Task.Delay(100);
            }

            return 0;
        }

        public static async Task<uint> InjectAsync(Process targetProcess, string targetDll, string procName)
        {
            uint threadExitCode = await RemoteLibaryFunction(targetProcess,
                lpModuleName: "kernel32.dll",
                lpProcName: "LoadLibraryA",
                lpParameters: Encoding.Default.GetBytes(targetDll),
                dwParamSize: (uint)((targetDll.Length + 1) * Marshal.SizeOf(typeof(char)))).ConfigureAwait(false);

            var remoteProcTask = RemoteLibaryFunction(targetProcess, targetDll, procName, null, 0);

            return await remoteProcTask;
        }
    }
}

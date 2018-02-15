using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static NInject.Kernel32.ProcessAccessFlags;
using static NInject.Kernel32.AllocationType;
using System.Threading;

namespace NInject
{
    public static class ProcessManager
    {
        private const uint STILL_ACTIVE = 259;

        private static readonly object lockObject = new object();

        private static Dictionary<Guid, ProcessInfo> processInfoDictionary;
        public static Guid[] ProcessInfos
        {
            get
            {
                lock (lockObject)
                {
                    var list = processInfoDictionary.Keys.ToArray();

                    return list;
                }
            }
        }

        static ProcessManager()
        {
            processInfoDictionary = new Dictionary<Guid, ProcessInfo>();
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hProcess">Process handle</param>
        /// <param name="lpModuleName">Module name</param>
        /// <param name="lpProcName">Proc name</param>
        /// <param name="lpParameters">Parameters</param>
        /// <param name="dwParamSize">Parameter size</param>
        /// <returns>Remote thread handle</returns>
        public static IntPtr RemoteLibaryFunction(IntPtr hProcess,
            string lpModuleName,
            string lpProcName,
            byte[] lpParameters,
            uint dwParamSize)
        {
            if (hProcess == IntPtr.Zero)
            {
                throw new ProcessException(ProcessException.Reasons.OpenProcessFault, hProcess, lpModuleName, lpProcName);
            }

            IntPtr lpRemoteParams;

            IntPtr hModule = Kernel32.GetModuleHandle(lpModuleName);

            IntPtr lpFunctionAddress = Kernel32.GetProcAddress(hModule, lpProcName);

            if (lpFunctionAddress == IntPtr.Zero)
            {
                hModule = Kernel32.LoadLibrary(lpModuleName);

                if (hModule == IntPtr.Zero)
                    throw new ProcessException(ProcessException.Reasons.LoadLibraryModuleNotFound, hProcess, lpModuleName, lpProcName);

                lpFunctionAddress = Kernel32.GetProcAddress(hModule, lpProcName);
            }

            if (lpFunctionAddress == IntPtr.Zero)
            {
                throw new ProcessException(ProcessException.Reasons.InvalidFunctionAdress, hProcess, lpModuleName, lpProcName);
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
                    throw new ProcessException(ProcessException.Reasons.RemoteParameterAllocationFault, hProcess, lpModuleName, lpProcName);
                }

                UIntPtr dwBytesWritten;

                bool result = Kernel32.WriteProcessMemory(hProcess,
                    lpRemoteParams,
                    lpParameters,
                    dwParamSize,
                    out dwBytesWritten);

                if (!result || dwBytesWritten == UIntPtr.Zero)
                {
                    throw new ProcessException(ProcessException.Reasons.WriteProcessMemoryFault, hProcess, lpModuleName, lpProcName);
                }
            }
            else
            {
                lpRemoteParams = IntPtr.Zero;
            }

            IntPtr hThread = Kernel32.CreateRemoteThread(hProcess,
                lpThreadAttributes: IntPtr.Zero,
                dwStackSize: 0,
                lpStartAddress: lpFunctionAddress,
                lpParameter: lpRemoteParams,
                dwCreationFlags: 0,
                lpThreadId: IntPtr.Zero);

            if (hThread == IntPtr.Zero)
            {
                throw new ProcessException(ProcessException.Reasons.CreateRemoteThreadFault, hProcess, lpModuleName, lpProcName);
            }

            return hThread;
        }

        public static uint GetExitCodeThread(IntPtr hThread)
        {
            uint dwOut;

            while (Kernel32.GetExitCodeThread(hThread, out dwOut))
            {
                if (dwOut != STILL_ACTIVE)
                {
                    break;
                }

                System.Threading.Thread.Sleep(10);
            }

            Kernel32.CloseHandle(hThread);

            return dwOut;
        }

        public static ProcessInfo Inject(Process targetProcess, string dllPath, string procName)
        {
            IntPtr hProcess = Kernel32.OpenProcess(
                CreateThread | VirtualMemoryOperation | VirtualMemoryRead | VirtualMemoryWrite | QueryInformation,
                false,
                targetProcess.Id);

            IntPtr hThread = RemoteLibaryFunction(hProcess,
                lpModuleName: "kernel32.dll",
                lpProcName: "LoadLibraryA",
                lpParameters: Encoding.Default.GetBytes(dllPath),
                dwParamSize: (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))));

            GetExitCodeThread(hThread);

            hThread = RemoteLibaryFunction(hProcess, dllPath, procName, null, 0);

            var processInfo = new ProcessInfo(targetProcess, hProcess, hThread);

            lock (lockObject)
            {
                processInfoDictionary.Add(processInfo.Id, processInfo);
            }

            return processInfo;
        }

        public static async Task CloseProcessAsync(Guid processInfoId)
        {
            ProcessInfo processInfo;

            lock (lockObject)
            {
                if (!processInfoDictionary.TryGetValue(processInfoId, out processInfo))
                    throw new ArgumentException();
            }

            const int cancelAfterMilliseconds = 1000;

            CancellationTokenSource cts;

            cts = new CancellationTokenSource(cancelAfterMilliseconds);

            Action waitForThreadExit = () => GetExitCodeThread(processInfo.ThreadHandle);

            await Task.Run(waitForThreadExit, cts.Token);

            lock (lockObject)
            {
                processInfoDictionary.Remove(processInfo.Id);
            }

            Kernel32.CloseHandle(processInfo.ProcessHandle);
        }
    }
}

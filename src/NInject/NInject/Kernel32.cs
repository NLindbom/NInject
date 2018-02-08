using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NInject
{
    public static class Kernel32
    {
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            //All                     = 0x001F0FFF,
            Terminate               = 0x00000001,
            CreateThread            = 0x00000002,
            VirtualMemoryOperation  = 0x00000008,
            VirtualMemoryRead       = 0x00000010,
            VirtualMemoryWrite      = 0x00000020,
            DuplicateHandle         = 0x00000040,
            CreateProcess           = 0x00000080,
            SetQuota                = 0x00000100,
            SetInformation          = 0x00000200,
            QueryInformation        = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize             = 0x00100000
        }

        [Flags]
        public enum AllocationType : uint
        {
            MemCommit       = 0x00001000,
            MemReserve      = 0x00002000,
            MemReset        = 0x00080000,
            MemResetUndo    = 0x01000000,
            MemLargePage    = 0x20000000,
            MemPhysical     = 0x00400000,
            MemTopDown      = 0x00100000
        }

        [Flags]
        public enum Protect : uint
        {
            PageExecute             = 0x10,
            PageExecuteRead         = 0x20,
            PageExecuteReadWrite    = 0x40,
            PageExecuteWriteCopy    = 0x80,
            PageNoAccess            = 0x01,
            PageReadOnly            = 0x02,
            PageReadWrite           = 0x04,
            PageWriteCopy           = 0x08,
            PageTargetsInvalid      = 0x40000000,
            PageTargetsNoUpdate     = 0x40000000,
            PageGuard               = 0x100,
            PageNocache             = 0x200,
            PageWritecombine        = 0x400
        }

        [DllImport("kernel32.dll")]
        public static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, out int size);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess, 
            IntPtr lpAddress, 
            uint dwSize, 
            AllocationType flAllocationType, 
            Protect flProtect
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            IntPtr lpThreadId
        );

        [DllImport("kernel32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpModuleName);
    }


}

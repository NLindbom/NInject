using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NInject
{
    public class ProcessException : Exception
    {
        public enum Reasons
        {
            OpenProcessFault,
            InvalidFunctionAdress,
            RemoteParameterAllocationFault,
            WriteProcessMemoryFault,
            CreateRemoteThreadFault,
            LoadLibraryModuleNotFound
        }

        public Reasons Reason { get; }

        public IntPtr ProcessHandle { get; }

        public string ModuleName { get; }

        public string ProcName { get; }

        public ProcessException(Reasons reason, IntPtr hProcess, string moduleName, string procName) : base($"{reason.ToString()}\nprocess handle: 0x{hProcess.ToString("x8")}\nmodule: {moduleName}\nproc: {procName}")
        {
            Reason = reason;
            ProcessHandle = hProcess;
            ModuleName = moduleName;
            ProcName = procName;
        }
    }
}

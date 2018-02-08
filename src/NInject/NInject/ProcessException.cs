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
            CreateRemoteThreadFault
        }

        public Reasons Reason { get; }

        public string ProcessName { get; }

        public string ModuleName { get; }

        public string ProcName { get; }

        public ProcessException(Reasons reason, Process process, string moduleName, string procName) : base($"{reason.ToString()} process: {process.ProcessName} module: {moduleName}, proc: {procName}")
        {
            Reason = reason;
            ProcessName = process.ProcessName;
            ModuleName = moduleName;
            ProcName = procName;
        }
    }
}

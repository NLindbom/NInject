using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NInject
{
    public class ProcessInfo
    {
        public Guid Id { get; }
        public Process Process { get; }
        public IntPtr ProcessHandle { get; set; }
        public IntPtr ThreadHandle { get; set; }

        public ProcessInfo(Process process, IntPtr hProcess, IntPtr hThread)
        {
            Id = Guid.NewGuid();
            Process = process;
            ProcessHandle = hProcess;
            ThreadHandle = hThread;
        }
    }
}

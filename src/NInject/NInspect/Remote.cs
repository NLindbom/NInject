using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NInspect
{
    public static class Remote
    {
        public static MainForm Client { get; private set; }

        [DllExport(nameof(Run), System.Runtime.InteropServices.CallingConvention.StdCall)]
        public static void Run()
        {
            using (var form = new MainForm())
            {
                Client = form;

                form.ShowDialog();

                Client = null;
            }
        }
    }
}

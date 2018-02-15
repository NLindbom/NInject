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
        [DllExport("Run", System.Runtime.InteropServices.CallingConvention.StdCall)]
        public static void Start()
        {
            using (var form = new MainForm())
            {
                form.ShowDialog();
            }
        }
    }
}

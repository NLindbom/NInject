using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NInspect
{
    public static class Test
    {
        
        [DllExport("TestA", System.Runtime.InteropServices.CallingConvention.StdCall)]
        public static void TestA()
        {
            MessageBox.Show("Hello.");
        }        
    }
}

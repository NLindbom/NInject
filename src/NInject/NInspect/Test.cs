using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NInspect
{
    public class Test
    {
        
        [DllExport("TestA", System.Runtime.InteropServices.CallingConvention.StdCall)]
        public static void TestA()
        {
        }        
    }
}

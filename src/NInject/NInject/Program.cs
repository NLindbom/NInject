using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NInject
{
    static class Program
    {
        public const string x86DllName = "NInspect.dll";
        public const string x64DllName = "NInspect64.dll";

        public static string DllPath { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string dllPath;

            if (IntPtr.Size == 8)
                dllPath = System.IO.Path.Combine(directory, x64DllName);
            else
                dllPath = System.IO.Path.Combine(directory, x86DllName);

            // dllPath = System.IO.Path.Combine(directory, x64DllName);
            // dllPath = System.IO.Path.Combine(directory, x86DllName);

            if (!System.IO.File.Exists(dllPath))
            {
                throw new System.IO.FileNotFoundException(dllPath);
            }

            DllPath = dllPath;

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.Run(new MainForm());            
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            StringBuilder message = new StringBuilder();

            message.AppendLine(e.Exception.Message);

            int win32Error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();

            if (win32Error != 0)
                message.AppendLine($"Win32 error code: {win32Error}");

            MessageBox.Show(message.ToString(), "Application ThreadException");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NInject
{
    static class Program
    {
        public const string dllName = "NInspect.dll";

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

            string dllPath = System.IO.Path.Combine(directory, dllName);

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
            int win32Error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();

            MessageBox.Show($"Application ThreadException: {e.Exception.Message} , Win32 error: {win32Error}");
        }
    }
}

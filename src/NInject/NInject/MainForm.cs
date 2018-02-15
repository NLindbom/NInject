using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NInject
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void openProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new SelectProcessForm(Process.GetProcesses()))
            {
                form.ShowDialog();

                if (form.DialogResult == DialogResult.OK)
                {
                    OpenProcess(form.Process);
                }
            }
        }

        private void OpenProcess(Process process)
        {
            var tabPage = new TabPage(process.ProcessName);

            tabControl.TabPages.Add(tabPage);

            var processInfo = ProcessManager.Inject(process, Program.DllPath, "Run");             
        }

        private void Close(Guid processInfoId)
        {

        }

        private void CloseAll()
        {
            var processInfoIds = ProcessManager.ProcessInfos;

            foreach (var processInfoId in processInfoIds)
            {
                Task.Run(() => ProcessManager.CloseProcessAsync(processInfoId));
            }
            // TODO: Free .dll
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
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

        private async void openProcessToolStripMenuItem_ClickAsync(object sender, EventArgs e)
        {
            using (var form = new SelectProcessForm(Process.GetProcesses()))
            {
                form.ShowDialog();

                if (form.DialogResult == DialogResult.OK)
                {
                    await OpenProcessAsync(form.Process);
                }
            }
        }

        private async Task OpenProcessAsync(Process process)
        {
            var tabPage = new TabPage(process.ProcessName);

            tabControl.TabPages.Add(tabPage);

            await ProcessManager.InjectAsync(process, Program.DllPath, "TestA");
        }
    }
}

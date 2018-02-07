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

        }
    }
}

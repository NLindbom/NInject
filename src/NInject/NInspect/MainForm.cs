using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NInspect
{
    public partial class MainForm : Form
    {
        private Control selectedControl = null;
        private Control SelectedControl
        {
            get
            {
                return selectedControl;
            }
            set
            {
                selectedControl = value;
                propertyGrid.SelectedObject = selectedControl;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            MouseHookManager.IgnoreHandle(this.Handle);

            try
            {
                MouseHookManager.SetHook();
            }
            catch (InvalidOperationException)
            {
                // Already hooked
            }

            MouseHookManager.MouseDown += MouseHookManager_MouseDown;
            MouseHookManager.MouseUp += MouseHookManager_MouseUp;

            this.Disposed += MainForm_Disposed;
        }

        private void MouseHookManager_MouseUp(object sender, MouseEventArgs e)
        {
            var hWnd = User32.WindowFromPoint(Cursor.Position);

            if (hWnd == IntPtr.Zero)
            {
                SelectedControl = null;
            }
            else
            {
                var control = Control.FromHandle(hWnd);

                SelectedControl = control;
            }
        }

        private void MouseHookManager_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void MainForm_Disposed(object sender, EventArgs e)
        {
            MouseHookManager.MouseDown -= MouseHookManager_MouseDown;
            MouseHookManager.MouseUp -= MouseHookManager_MouseUp;
        }
    }
}

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

namespace NInspect
{
    public partial class MainForm : Form
    {
        private Process process;

        private Control selectedControl = null;
        private Control SelectedControl
        {
            get { return selectedControl; }
            set
            {
                selectedControl = value;
                propertyGrid.SelectedObject = selectedControl;
            }
        }

        public MainForm(Process injectedProcess)
        {
            this.process = injectedProcess;

            InitializeComponent();

            SetMouseHook();

            windowHandleBrowser1.SetMainWindowHandle(injectedProcess.MainWindowHandle);
        }

        private void SetMouseHook()
        {
            try
            {
                MouseHookManager.SetHook();
            }
            catch (InvalidOperationException)
            {
                // Already hooked
            }

            MouseHookManager.IgnoreInsideWindowHandle(this.Handle);

            MouseHookManager.MouseDown += MouseHookManager_MouseDown;
            MouseHookManager.MouseUp += MouseHookManager_MouseUp;

            this.Disposed += (s, e) =>
            {
                MouseHookManager.MouseDown -= MouseHookManager_MouseDown;
                MouseHookManager.MouseUp -= MouseHookManager_MouseUp;
            };
        }

        private void PopulateListView()
        {
            using (var process = Process.GetCurrentProcess())
            {
                IntPtr hWnd = process.MainWindowHandle;
            }
        }

        private void MouseHookManager_MouseUp(object sender, MouseEventArgs e)
        {
            var hWnd = User32.WindowFromPoint(Cursor.Position);

            if (hWnd == IntPtr.Zero)
            {
                SelectedControl = null;

                label1.Text = "-";
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

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var item = e.ChangedItem;
        }
    }
}

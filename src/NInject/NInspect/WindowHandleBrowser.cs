using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace NInspect
{
    public partial class WindowHandleBrowser : UserControl
    {
        private IntPtr mainWindowHandle = IntPtr.Zero;

        public WindowHandleBrowser()
        {
            InitializeComponent();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            PopulateWindowHandles();
        }

        public void SetMainWindowHandle(IntPtr hWnd)
        {
            mainWindowHandle = hWnd;

            PopulateWindowHandles();
        }

        public void PopulateWindowHandles()
        {
            treeView.Nodes.Clear();

            var node = CreateNode(mainWindowHandle);

            treeView.Nodes.Add(node);

            AddChildren(mainWindowHandle, node.Nodes, 10);

            treeView.ExpandAll();
        }

        private TreeNode CreateNode(IntPtr handle)
        {
            TreeNode node;

            var control = FromHandle(handle);

            if (control == null)
            {
                var text = GetControlText(handle);

                if (!string.IsNullOrEmpty(text))
                {
                    node = new TreeNode("(" + GetClassName(handle) + ") hWnd text: " + GetControlText(handle) + " [0x" + handle.ToString("x2") + "]");
                    node.ForeColor = Color.Black;
                }
                else
                {
                    node = new TreeNode("(" + GetClassName(handle) + ") [0x" + handle.ToString("x2") + "]");
                    node.ForeColor = Color.Gray;
                }
            }
            else
            {
                node = new TreeNode("(" + control.GetType() + ";" + GetClassName(handle) + ") " + control.Name + ".Text = " + control.Text + " [0x" + handle.ToString("x2") + "]");
                node.ForeColor = Color.DarkBlue;
            }

            return node;
        }

        private string GetClassName(IntPtr hWnd)
        {
            int nRet;
            var name = new StringBuilder(256);

            nRet = User32.GetClassName(hWnd, name, name.Capacity);

            if (nRet == 0)
                return string.Empty;

            return name.ToString();
        }

        private void AddChildren(IntPtr hWnd, TreeNodeCollection nodes, int depth)
        {
            const int maxChildren = 100;

            depth--;

            if (depth <= 0)
                return;

            nodes.Clear();

            var handles = GetAllChildrenWindowHandles(hWnd, maxChildren);

            foreach (var handle in handles)
            {
                var node = CreateNode(handle);

                nodes.Add(node);

                AddChildren(handle, node.Nodes, depth);

                if (treeView.Nodes.Count == maxChildren)
                {
                    var brNode = nodes.Add("////////");
                    brNode.ForeColor = Color.Red;

                    break;
                }
            }
        }

        public string GetControlText(IntPtr hWnd)
        {
            StringBuilder title = new StringBuilder();

            const int WM_GETTEXT = 0x000D;
            const int WM_GETTEXTLENGTH = 0x000E;

            // Get the size of the string required to hold the window title. 
            int size = User32.SendMessage(hWnd, WM_GETTEXTLENGTH, 0, null);

            // If the return is 0, there is no title. 
            if (size > 0)
            {
                title = new StringBuilder(size + 100);

                User32.SendMessage(hWnd, WM_GETTEXT, title.Capacity, title);
            }
            return title.ToString();
        }

        public List<IntPtr> GetAllChildrenWindowHandles(IntPtr hParent, int maxCount)
        {
            List<IntPtr> result = new List<IntPtr>();

            var previousChild = IntPtr.Zero;
            var currentChild = IntPtr.Zero;

            int count = 0;

            while (count < maxCount)
            {
                currentChild = User32.FindWindowEx(hParent, previousChild, null, null);

                if (currentChild == IntPtr.Zero)
                    break;

                result.Add(currentChild);

                previousChild = currentChild;

                count++;
            }

            return result;
        }
    }
}

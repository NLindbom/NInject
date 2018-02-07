using System;
using System.Collections;
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
    public partial class SelectProcessForm : Form
    {    
        public Process Process { get; private set; } = null;

        public SelectProcessForm(Process[] processes)
        {
            InitializeComponent();

            PopulateProcesses(processes);
        }

        private void PopulateProcesses(Process[] processes)
        {
            listViewProcesses.SmallImageList = new ImageList();

            foreach (var process in processes)
            {
                var executablePath = ProcessManager.GetExecutablePath(process);

                var filename = System.IO.Path.GetFileName(executablePath);
                var path = System.IO.Path.GetDirectoryName(executablePath);

                ListViewItem item;

                Icon icon;

                if (path == null)
                {
                    icon = null;
                }
                else
                {
                    icon = Icon.ExtractAssociatedIcon(executablePath);
                }

                if (icon == null)
                {
                    item = new ListViewItem(new string[] { null, process.Id.ToString(), process.ProcessName, process.MainWindowTitle, filename, path });
                }
                else
                {
                    listViewProcesses.SmallImageList.Images.Add(path, icon);

                    item = new ListViewItem(new string[] { null, process.Id.ToString(), process.ProcessName, process.MainWindowTitle, filename, path }, path);
                }

                item.Tag = process;

                listViewProcesses.Items.Add(item);
            }
        }

        private ListViewItemComparer GetListViewItemSorter(int columnIndex)
        {
            var sorter = (ListViewItemComparer)listViewProcesses.ListViewItemSorter;

            if (sorter == null || sorter.ColumnIndex != columnIndex)
            {
                sorter = new ListViewItemComparer(columnIndex);
            }

            if (sorter.Order == ListViewItemComparer.SortOrder.Ascending)
            {
                sorter.Order = ListViewItemComparer.SortOrder.Descending;
            }
            else
            {
                sorter.Order = ListViewItemComparer.SortOrder.Ascending;
            }

            return sorter;
        }

        private class ListViewItemComparer : IComparer
        {
            public enum SortOrder
            {
                None,
                Ascending,
                Descending
            }

            public int ColumnIndex { get; }
            public SortOrder Order { get; set; }

            public ListViewItemComparer(int column)
            {
                ColumnIndex = column;
            }

            public int Compare(object x, object y)
            {
                var item1 = x as ListViewItem;
                var item2 = y as ListViewItem;

                int result;

                if (item1 == null && item2 == null)
                {
                    result = 0;
                }
                else if (item1 == null)
                {
                    result = -1;
                }
                else if (item2 == null)
                {
                    result = 1;
                }

                result = string.Compare(item1.SubItems[ColumnIndex].Text, item2.SubItems[ColumnIndex].Text, false);

                if (Order == SortOrder.Descending)
                    return -result;

                return result;
            }
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void listViewProcesses_DoubleClick(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void CloseForm()
        {
            if (listViewProcesses.SelectedItems.Count == 0)
            {
                return;
            }

            this.Process = listViewProcesses.SelectedItems[0].Tag as Process;

            this.DialogResult = DialogResult.OK;

            this.Close();
        }
    }
}

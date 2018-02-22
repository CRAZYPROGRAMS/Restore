using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restore { 
    public partial class Viewer : Form {
        Dictionary<TreeNode, IDirItem> Items = new Dictionary<TreeNode, IDirItem>();
        Dictionary<IDirItem, TreeNode>  Nodes = new Dictionary<IDirItem, TreeNode>();
        Dictionary<ListViewItem, IDirItem> LItems = new Dictionary<ListViewItem, IDirItem>();
        Storage storage;
        private IDirItem restoreItem;
        ListViewGroup GroupFile, GroupDir;
        public Viewer(string path) {
            InitializeComponent();
            GroupDir = new ListViewGroup("Папки");
            GroupFile = new ListViewGroup("Файлы");
            listView1.Groups.Add(GroupDir);
            listView1.Groups.Add(GroupFile);
            this.storage = new Storage(path);
            showDir(treeView1.Nodes, this.storage.root.Childs());
            foreach (TreeNode node in treeView1.Nodes)
                if (Items[node].IsDir())
                    showDir(node.Nodes, Items[node].Childs());
        }
        private void showDir(TreeNodeCollection Nodes, IEnumerable<IDirItem> Childs,bool DirOnly = true) {
            foreach (IDirItem item in Childs)
                if (!DirOnly || item.IsDir())
                {
                    var node = new TreeNode();
                    this.Items[node] = item;
                    this.Nodes[item] = node;
                    node.Text = item.Name();
                    node.Name = item.Name();
                    /*if (item.IsDir())
                        showDir(node.Nodes, item.Childs());*/
                    Nodes.Add(node);
                }
        }
        private void showItem(TreeNode Node) {
            foreach (TreeNode node in Node.Nodes)
                if (Items[node].IsDir())
                    if (node.Nodes.Count == 0)
                        showDir(node.Nodes, Items[node].Childs());
        }
        private void showList(IDirItem node) {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            LItems.Clear();
            if (node.IsDir()) {
                foreach (IDirItem di in node.Childs()) {
                    var item = new ListViewItem();
                    LItems.Add(item, di);
                    item.Name = di.Name();
                    item.Text = di.Name();
                    var itemSize = new ListViewItem.ListViewSubItem();
                    var itemType = new ListViewItem.ListViewSubItem();
                    var itemCount = new ListViewItem.ListViewSubItem();
                    itemSize.Name = di.Name();
                    itemSize.Text = "";
                    itemType.Name = di.Name();
                    itemType.Text = "";
                    itemCount.Name = di.Name();
                    itemCount.Text = "1";
                    if (di is ISize) 
                        itemSize.Text = SizeStr(((ISize)di).Size());
                    if (di is ICount)
                        itemCount.Text = ((ICount)di).Count().ToString() ;
                    var group = GroupFile;
                    if (di is PointFile)
                    {
                        itemType.Text = "Файл";
                        group = GroupFile;
                    }
                    else if (di is PointTree)
                    {
                        itemType.Text = "Точка востановления"; 
                        group = GroupDir;
                    }
                    else if (di.IsDir())
                    {
                        itemType.Text = "Папка";
                        group = GroupDir;
                    }
                    item.Group = group;
                    item.SubItems.Add(itemSize);
                    item.SubItems.Add(itemType);
                    item.SubItems.Add(itemCount);
                    listView1.Items.Add(item);
                }
            }
            listView1.EndUpdate();
        }
        private void Form1_Load(object sender, EventArgs e) {

        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e) {
            showItem(e.Node);
        }

        static string SizeStr(Int64 size) {
            if (size > ((Int64)1 << 30)) {
                var G = ((double)size) / ((double)((Int64)1 << 30));
                return G.ToString("F1") + "GB";
            }
            if (size > ((Int64)1 << 20)) {
                var M= ((double)size) / ((double)((Int64)1 << 20));
                return M.ToString("F1") + "MB";
            }
            if (size > ((Int64)1 << 10)) {
                var K = ((double)size) / ((double)((Int64)1 << 10));
                return K.ToString("F1") + "KB";
            }
            return size.ToString("F1") + "B";
        }
        private void SelectItem(IDirItem item,bool open = true) {
            restoreItem = item;
            if (open && this.Nodes.ContainsKey(item)) {
                var node = Nodes[item];
                treeView1.SelectedNode = node;
                showItem(node);
            }
            restoreItem = item;
            statusName.Text = item.Name();
            if (item is PointFile)
                statusTypeObject.Text = "Файл";
            else if (item is PointDir)
                statusTypeObject.Text = "Папка";
            else if (item is PointTree)
                statusTypeObject.Text = "Точка востановления";
            else
                statusTypeObject.Text = "";
            if (item is ISize)
                statusSize.Text = SizeStr(((ISize)item).Size());
            else
                statusSize.Text = "";
            if (item is ICount)
                statusCount.Text = ((ICount)item).Count().ToString();
            else
                statusCount.Text = "";
            if (open)
                    showList(item);
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e) {
            SelectItem(Items[e.Node], true);
        }
        static DateTime OldSwowTime = DateTime.Now;
        private void StorageEventHandler(Storage storage, Storage.StatRestore stat) {
            Int64 MaxPoints = 0;
            Int64 Points = 0;
            Int64 FilePoint = 4096;
            Int64 BytePoint = 1;
            if (stat.Item is ICount) {
                MaxPoints += ((ICount)stat.Item).Count() * FilePoint;
                Points += stat.NumFiles * FilePoint;
            }
            if (stat.Item is ISize) {
                MaxPoints += ((ISize)stat.Item).Size() * BytePoint;
                Points += stat.SizeFiles * BytePoint;
            }
            if (DateTime.Now.Subtract(OldSwowTime).TotalMilliseconds < 0)
                OldSwowTime = DateTime.Now;
            if (DateTime.Now.Subtract(OldSwowTime).TotalMilliseconds > 1000) {
                statusProgress.Maximum = 1000;
                statusProgress.Value = (int)(Points * 1000 / MaxPoints);
                statusProgress.Minimum = 0;
                OldSwowTime = DateTime.Now;
                Application.DoEvents();
            }
        }
        private void menuFileRestore_Click(object sender, EventArgs e) {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok) {
                var stat = new Storage.StatRestore();
                stat.Item = this.restoreItem;
                stat.OnChange += StorageEventHandler;
                this.Enabled = false;
                statusProgress.Visible = true;
                this.storage.RestoreAll(this.restoreItem, Path.Combine(dlg.FileName, this.restoreItem.Name()), stat);
                statusProgress.Visible = false;
                this.Enabled = true;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
                SelectItem(LItems[listView1.SelectedItems[0]], false);
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(listView1.SelectedItems.Count == 1)
                SelectItem(LItems[listView1.SelectedItems[0]], true);
        }

        private void menuFileExit_Click(object sender, EventArgs e) {
            Close();
        }
    }
}

using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restore
{ 
    public partial class Viewer : Form
    {
        Dictionary<TreeNode, IDirItem> Items = new Dictionary<TreeNode, IDirItem>();
        private IDirItem restoreItem;
        private string storePath;
        IDirItem root;
        class StatRestore {
            int NumFile;
            Int64 NumSize;
        }
        ListViewGroup GroupFile, GroupDir;
        public Viewer(string path)
        {
            storePath = path;
            InitializeComponent();
            GroupDir = new ListViewGroup("Папки");
            GroupFile = new ListViewGroup("Файлы");
            listView1.Groups.Add(GroupDir);
            listView1.Groups.Add(GroupFile);
            
            root = new FSDirectory(Path.Combine(path, "points"), "", null);
            showDir(treeView1.Nodes, root.Childs());
            foreach (TreeNode node in treeView1.Nodes)
                if (Items[node].IsDir())
                    showDir(node.Nodes, Items[node].Childs());
        }
        private void showDir(TreeNodeCollection Nodes, IEnumerable<IDirItem> Childs,bool DirOnly = true) {
            foreach (IDirItem item in Childs)
                if (!DirOnly || item.IsDir())
                {
                    var node = new TreeNode();
                    Items[node] = item;
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
            if (node.IsDir()) {
                foreach (IDirItem di in node.Childs()) {
                    var item = new ListViewItem();
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
        private void restore(IDirItem node, string path, StatRestore stat) {
            
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            showItem(e.Node);
        }

        static string SizeStr(Int64 size) {
            if (size > ((Int64)1 << 30)) {
                var G = ((double)size) / ((double)((Int64)1 << 30));
                return G.ToString("F1") + "GB";
            }
            if (size > ((Int64)1 << 20))
            {
                var M= ((double)size) / ((double)((Int64)1 << 20));
                return M.ToString("F1") + "MB";
            }
            if (size > ((Int64)1 << 10))
            {
                var K = ((double)size) / ((double)((Int64)1 << 10));
                return K.ToString("F1") + "KB";
            }
            return size.ToString("F1") + "B";
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            restoreItem = Items[e.Node];
            showItem(e.Node);
            if (Items[e.Node] is PointFile)
            {
                statusTypeObject.Text = "Файл";
            }
            else if (Items[e.Node] is PointDir)
            {
                statusTypeObject.Text = "Папка";
                
            }
            else if (Items[e.Node] is PointTree)
            {
                statusTypeObject.Text = "Точка востановления";
            }
            else
            {
                statusTypeObject.Text = "";
                
            }
            if (Items[e.Node] is ISize) 
                statusSize.Text = SizeStr(((ISize)Items[e.Node]).Size());
            else
                statusSize.Text = "";
            if (Items[e.Node] is ICount)
                statusCount.Text = ((ICount)Items[e.Node]).Count().ToString();
            else
                statusCount.Text = "";
            showList(Items[e.Node]);
        }

        private void menuFileRestore_Click(object sender, EventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok) {
                var stat = new StatRestore();
                this.restore(this.restoreItem, dlg.FileName, stat);
            }
            
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

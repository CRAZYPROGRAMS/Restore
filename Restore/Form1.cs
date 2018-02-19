using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Restore
{
    public partial class Form1 : Form
    {
        Dictionary<TreeNode, IDirItem> Items = new Dictionary<TreeNode, IDirItem>();
        IDirItem root;
        public Form1()
        {
            InitializeComponent();
            root = new FSDirectory("D:/Storage/points/", "", null);
            showDir(treeView1.Nodes, root.Childs());
            foreach (TreeNode node in treeView1.Nodes)
                if (Items[node].IsDir())
                    showDir(node.Nodes, Items[node].Childs());
        }
        private void showDir(TreeNodeCollection Nodes, IEnumerable<IDirItem> Childs) {
            foreach (IDirItem item in Childs)
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
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            
            foreach (TreeNode node in e.Node.Nodes)
                if (Items[node].IsDir())
                    if (node.Nodes.Count==0)
                        showDir(node.Nodes, Items[node].Childs());
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
        static string SizeStr(Int64 size) {
            var B = (size % 1024);
            var K = ((size / 1024) % 1024);
            var M = ((size / 1024 / 1024) % 1024);
            var G = ((size / 1024 / 1024 / 1024));
            var S = "";
            if (G > 0)
                S += G + "GB ";
            if (G > 0 || M > 0)
                S += M + "MB ";
            if (G > 0 || M > 0 || K > 0)
                S += K + "KB ";
            S += B + "B ";
            return size.ToString()+"( "+ S +")";
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
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
        }

        private void menuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

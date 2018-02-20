using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restore
{
    class PointDir:IDirItem, ISize, IAddSize, IAddChild, ICount
    {
        private PointTree tree;
        private Dictionary<string, IDirItem> childs = new Dictionary<string, IDirItem>();
        private string path;
        private string name;
        private Int64 size;
        private Int64 count;
        private IDirItem parent;
        public bool IsDir() { return true; }
        public IDirItem Parent() { return this.parent; }
        public string Name() { return this.name; }
        public string Path() { return this.parent == null ? this.path : this.tree.Path() + "/" + this.path; }
        public PointDir(string path, PointTree tree)
        {
            this.path = path;
            this.tree = tree;
            this.name = System.IO.Path.GetFileName(path);
            var dir = System.IO.Path.GetDirectoryName(path);
            if (dir != "")
            {
                var pdir = tree.getDir(dir);
                pdir.AddChild(this.name, this);
                parent = pdir;
            }
            else
            {
                parent = tree;
                tree.AddChild(this.name, this);
            }
            
        }
        public Int64 Count() { return this.count; }
        public void AddSize(Int64 size) {
            this.count++;
            this.size += size;
            var pdir = this.parent as PointDir;
            if (pdir != null)
                pdir.AddSize(size);
            else if (this.parent is PointTree)
                tree.AddSize(size);                    
        }
        public void AddChild(string name, IDirItem item) {
            childs[name] = item;
        }
        public Int64 Size() { return size; }
        public IEnumerable<IDirItem> Childs()
        {
            return childs.Values;
        }
    }
}

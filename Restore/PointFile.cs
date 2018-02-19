using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restore
{
    class PointFile: IDirItem, ISize
    {
        private PointTree tree;
        private string path;
        private string name;
        private Int64 size;
        private IDirItem parent;
        
        public bool IsDir() { return false; }
        public IDirItem Parent() { return this.parent; }
        public string Name() { return this.name; }
        public string Path() { return this.parent == null ? this.name : this.parent.Path() + "/" + this.name; }
        public PointFile(string path, string hash, Int64 size, PointTree tree)
        {
            this.size = size;
            var dir = System.IO.Path.GetDirectoryName(path);
            name = System.IO.Path.GetFileName(path);
            if (dir != "")
                parent = tree.getDir(dir);
            else
                parent = tree;
            if (parent is IAddSize)
                ((IAddSize)parent).AddSize(size);
            if (parent is IAddChild)
                ((IAddChild)parent).AddChild(name, this);

        }
        public Int64 Size() { return size; }
        public IEnumerable<IDirItem> Childs()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restore
{
    class PointTree : IDirItem, ISize, IAddSize, IAddChild
    {
        private Dictionary<string, IDirItem> childs = new Dictionary<string, IDirItem>();
        private string path;
        private string name;
        private Int64 size;
        private Dictionary<string, IDirItem> files = new Dictionary<string, IDirItem>();
        private IDirItem parent;
        public bool IsDir() { return true; }
        public IDirItem Parent() { return this.parent; }
        public string Name() { return this.name; }
        public string Path() { return this.parent == null ? this.name : this.parent.Path() + "/" + this.name; }
        private bool load = false;
        private void loadFile() {
            bool FirstLine = true;
            var poles = new Dictionary<string, int>();
            using (StreamReader sr = new StreamReader(path))
            {
                while (sr.Peek() >= 0)
                {
                    string[] FileLine = sr.ReadLine().Split('\t');
                    if (FirstLine)
                    {
                        FirstLine = false;
                        for (var i = 0; i < FileLine.Length; i++)
                            poles.Add(FileLine[i].ToLower(), i);
                    }
                    else
                    {
                        string file_path = FileLine[poles["path"]];
                        string file_hash = FileLine[poles["hash"]].ToLower();
                        Int64 file_size = 0;
                        if (Int64.TryParse(FileLine[poles["size"]], out file_size))
                            files.Add(file_path, new PointFile(file_path, file_hash, file_size, this));
                    }
                }
            }
            this.load = true;
        }
        public PointTree(string path, string name, IDirItem parent) {
            this.path = path;
            this.name = name;
            this.parent = parent;
        }
        public PointDir getDir(string path) {
            if (!files.ContainsKey(path))
                files[path] = new PointDir(path, this);
            return files[path] as PointDir;
        }
        public void AddChild(string name, IDirItem item)
        {
            childs[name] = item;
        }
        public IEnumerable<IDirItem> Childs()
        {
            if (!this.load)
                loadFile();
            return childs.Values;
        }
        public Int64 Size() { return size; }
        public void AddSize(Int64 size)
        {
            this.size += size;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restore
{
    class FSDirectory: IDirItem {
        private Dictionary<string, IDirItem> cachePoint = new Dictionary<string, IDirItem>();
        private string path;
        private string name;
        private IDirItem parent;
        public FSDirectory(string path, string name, IDirItem parent) {
            this.path = path;
            this.name = name;
            this.parent = parent;
        }
        public bool IsDir() { return true; }
        public IDirItem Parent(){ return this.parent; }
        public string Name() { return this.name; }
        public string Path() { return this.parent==null? this.name: this.parent.Path()+"/"+this.name; }
        public IEnumerable<IDirItem> Childs() {
            var newCachePoint = new Dictionary<string, IDirItem>();
            var result = new List<IDirItem>();
            foreach (string subDir in Directory.EnumerateDirectories(path)) {
                IDirItem item;
                if (cachePoint.ContainsKey(subDir))
                    item = cachePoint[subDir];
                else
                    item = new FSDirectory(subDir, System.IO.Path.GetFileName(subDir), this);
                newCachePoint[subDir] = item;
            }
            foreach (string subDir in Directory.EnumerateFiles(path))
                if (System.IO.Path.GetExtension(subDir).ToLower()!=".log") {
                    IDirItem item;
                    if (cachePoint.ContainsKey(subDir))
                        item = cachePoint[subDir];
                    else
                        item = new PointTree(subDir, System.IO.Path.GetFileName(subDir), this);
                    newCachePoint[subDir] = item;
                }
            cachePoint = newCachePoint;
            return newCachePoint.Values;
        }
    }
}

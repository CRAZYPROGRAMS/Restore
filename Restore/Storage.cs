using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Restore
{
    class Storage {
        public delegate void StorageEventHandler(Storage storage, StatRestore stat);
        public class StatRestore {
            public IDirItem Item;
            public Int64 NumFiles;
            public Int64 SizeFiles;
            public Int64 NumFaildFiles;
            public event StorageEventHandler OnChange  = new StorageEventHandler((e,s)=> { });
            public void Change(Storage storage) { OnChange(storage, this); }
        }
        private string storegePath;
        public IDirItem root { get; private set; }
        public Storage(string storegePath) {
            this.storegePath = storegePath;
            this.root = new FSDirectory(Path.Combine(storegePath, "points"), "", null);
        }
        public bool RestoreAll(IDirItem item, string path, StatRestore stat = null) {
            if (!Restore(item, path, stat))
                return false;
            if (item.IsDir()) {
                foreach (IDirItem subitem in item.Childs()) {
                    if (!RestoreAll(subitem, Path.Combine(path, subitem.Name()), stat))
                        return false;
                }
            }
            return true;
        }
        private string SizeToInvHexByte(Int64 Size, int lenByte) {
            char[] A = new char[lenByte * 2];
            var lit = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            for (int i = 0; i < lenByte; i++) {
                A[i * 2 + 0] = lit[(Size / 16) % 16];
                A[i * 2 + 1] = lit[(Size) % 16];
                Size = Size / 256;
            }
            return new String(A);
        }
        public bool Restore(IDirItem item, string path, StatRestore stat = null) {
            if (item.IsDir())
            {
                if (Directory.Exists(path))
                    return true;
                Directory.CreateDirectory(path);
                return true;
            }
            else if (item is PointFile) {
                var file = (PointFile)item;
                if (File.Exists(path))
                    return false;
                var filePath = Path.Combine(storegePath, file.hash.Substring(file.hash.Length - 2, 2), file.hash.Substring(file.hash.Length - 4, 2), file.hash+"."+SizeToInvHexByte(file.Size(), 8));
                if (File.Exists(filePath + ".bin")) {
                    using (var rfile = new FileStream(filePath + ".bin", FileMode.Open, FileAccess.Read))
                    using (var wfile = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                        rfile.CopyTo(wfile);
                    if (stat != null) { stat.NumFiles++; stat.SizeFiles += file.Size(); stat.Change(this); }
                } else if (File.Exists(filePath + ".gzip")) {
                    using (var rofile = new FileStream(filePath + ".gzip", FileMode.Open, FileAccess.Read))
                    using (var rfile = new GZipStream(rofile, CompressionMode.Decompress))
                    using (var wfile = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                        rfile.CopyTo(wfile);
                    if (stat != null) { stat.NumFiles++; stat.SizeFiles += file.Size(); stat.Change(this); }
                } else {
                    if (stat != null) { stat.NumFaildFiles++; stat.Change(this); }
                    return false;
                }
            }
            return true;
        }
    }
}

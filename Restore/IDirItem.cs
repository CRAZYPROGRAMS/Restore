using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restore
{
    interface IDirItem
    {
        IEnumerable<IDirItem> Childs();
        string Path();
        IDirItem Parent();
        string Name();
        bool IsDir();
     }
    interface IAddChild {
        void AddChild(string name, IDirItem item);
    }
}

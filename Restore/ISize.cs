﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restore { 
    interface ISize {
        Int64 Size();
    }
    interface ICount {
        Int64 Count();
    }
    interface IAddSize {
        void AddSize(Int64 size);
    }
}

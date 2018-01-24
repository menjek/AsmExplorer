using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmExplorer
{
    class AsmFile
    {
        public string Path { get; set; }
        public AsmFunction[] Functions { get; set; }
    }
}

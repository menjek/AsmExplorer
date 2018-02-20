using System.Collections.Generic;

namespace VSAsm
{
    public class AsmFile
    {
        public string Path { get; set; }
        public List<AsmFunction> Functions { get; set; }
    }
}

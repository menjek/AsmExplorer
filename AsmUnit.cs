using System.Collections.Generic;

namespace VSAsm
{
    public class AsmUnit
    {
        public string Name { get; set; }
        public Dictionary<string, AsmFile> Files { get; set; }
    }
}

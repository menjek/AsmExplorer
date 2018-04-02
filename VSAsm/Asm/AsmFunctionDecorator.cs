using System.Collections.Generic;
using System.Windows.Documents;

namespace VSAsm
{
    public class AsmFunctionDecorator
    {
        public AsmFunction Function { get; set; }
        public Run Run { get; set; }

        public List<AsmBlockDecorator> Blocks { get; set; }
    }
}

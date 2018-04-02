using System.Windows.Documents;
using System.Collections.Generic;

namespace VSAsm
{
    public class AsmBlockDecorator
    {
        public AsmBlock Block { get; set; }
        public Span Span { get; set; }

        public List<AsmInstructionDecorator> Instructions { get; set; }
    }
}

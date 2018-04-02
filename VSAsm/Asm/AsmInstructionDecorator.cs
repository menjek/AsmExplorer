using System.Collections.Generic;
using System.Windows.Documents;

namespace VSAsm
{
    public class AsmInstructionDecorator
    {
        public AsmInstruction Instruction { get; set; }
        public Span Span { get; set; }
        public Run Name { get; set; }
        public List<AsmInstructionArgDecorator> Args { get; set; }
        public Run Comment { get; set; }
    }
}

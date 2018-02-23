namespace VSAsm
{
    public class AsmInstructionConstantArg : IAsmInstructionArg
    {
        public InstructionArgType Type {
            get { return InstructionArgType.Constant; }
        }

        public long Value { get; set; }
    }
}

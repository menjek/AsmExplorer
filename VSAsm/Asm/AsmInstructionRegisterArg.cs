namespace VSAsm
{
    public class AsmInstructionRegisterArg : IAsmInstructionArg
    {
        public InstructionArgType Type {
            get { return InstructionArgType.Register; }
        }

        public string Name { get; set; }
    }
}

namespace VSAsm
{
    public class AsmInstructionLabelArg : IAsmInstructionArg
    {
        public InstructionArgType Type {
            get { return InstructionArgType.Label; }
        }

        public string Prefix { get; set; }
        public string Name { get; set; }
    }
}

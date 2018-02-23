namespace VSAsm
{
    public class AsmInstructionIndirectAddressArg : IAsmInstructionArg
    {
        public InstructionArgType Type {
            get { return InstructionArgType.IndirectAddress; }
        }

        public string Unparsed { get; set; }
    }
}

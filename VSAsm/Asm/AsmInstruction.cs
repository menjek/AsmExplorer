namespace VSAsm
{
    public class AsmInstruction
    {
        public ulong Address { get; set; }
        public byte[] OpCode { get; set; }
        public string Name { get; set; }
        public IAsmInstructionArg[] Args { get; set; }
        public string Comment { get; set; }
    }
}

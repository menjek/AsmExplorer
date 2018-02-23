namespace VSAsm
{
    public enum InstructionArgType
    {
        Constant,
        Register,
        IndirectAddress,
        Label
    }

    public interface IAsmInstructionArg
    {
        InstructionArgType Type { get; }
    }
}

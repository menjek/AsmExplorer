namespace VSAsm
{
    public class AsmInstructionIndirectAddressArg : IAsmInstructionArg
    {
        public class IndexArg
        {
            public string Index { get; set; }
            public string Scale { get; set; }
            public string Displacement { get; set; }
        }

        public string Base { get; set; }
        public IndexArg[] Indexes { get; set; }
        public string Displacement { get; set; }
    }
}

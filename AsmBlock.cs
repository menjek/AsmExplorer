namespace VSAsm
{
    class AsmBlock
    {
        public int SourceStartLine { get; set; }
        public int SourceEndLine { get; set; }
        public string[] Assembly { get; set; }
    }
}

namespace VSAsm
{
    class AsmFunction
    {
        public string Name { get; set; }
        public string MangledName { get; set; }
        public AsmBlock[] Blocks { get; set; }
    }
}

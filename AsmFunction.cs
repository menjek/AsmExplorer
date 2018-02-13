using System;

namespace VSAsm
{
    public class AsmFunction : IComparable<AsmFunction>
    {
        public string Name { get; set; }
        public string MangledName { get; set; }
        public LineRange Range { get; set; }
        public AsmBlock[] Blocks { get; set; }

        public int CompareTo(AsmFunction rhs)
        {
            return Range.CompareTo(rhs.Range);
        }
    }
}

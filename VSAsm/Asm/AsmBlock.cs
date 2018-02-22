using System;

namespace VSAsm
{
    public class AsmBlock : IComparable<AsmBlock>
    {
        public LineRange Range { get; set; }
        public AsmInstruction[] Instructions { get; set; }

        public int CompareTo(AsmBlock rhs)
        {
            return Range.CompareTo(rhs.Range);
        }
    }
}

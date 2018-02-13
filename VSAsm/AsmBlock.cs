using System;

namespace VSAsm
{
    public class AsmBlock : IComparable<AsmBlock>
    {
        public LineRange Range { get; set; }
        public string[] Assembly { get; set; }

        public int CompareTo(AsmBlock rhs)
        {
            return Range.CompareTo(rhs.Range);
        }
    }
}

using System;

namespace VSAsm
{
    public struct LineRange : IComparable<LineRange>
    {
        public static readonly LineRange InvalidRange = new LineRange(int.MaxValue, int.MinValue);

        public int Min { get; set; }
        public int Max { get; set; }

        public bool IsValid {
            get { return (Min <= Max); }
        }

        public int Size {
            get { return (Max - Min); }
        }

        public LineRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public bool Contains(int line)
        {
            return (Min <= line) && (line <= Max);
        }

        public int CompareTo(LineRange other)
        {
            return Min.CompareTo(other.Min);
        }
    }
}

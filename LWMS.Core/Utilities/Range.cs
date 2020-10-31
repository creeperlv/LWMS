using System;

namespace LWMS.Core.Utilities
{
    public struct Range
    {
        public static Range Empty = new Range() { L = long.MinValue, R = long.MinValue };
        public long L;
        public long R;
        public override string ToString()
        {
            if (L == long.MinValue)
            {
                if (R == long.MinValue)
                    return $"-";
                return $"-{R}";
            }
            else if (R == long.MinValue)
                return $"{L}-";
            else return $"{L}-{R}";
        }
        public override bool Equals(object obj)
        {
            if (obj is Range)
            {
                var item = (Range)obj;
                return item.R == R && item.L == L;
            }
            else
                return base.Equals(obj);
        }
        public static Range FromString(string str)
        {
            str = str.Trim();
            var g = str.Split('-');
            Range range = new Range();
            if (g[0] == null || g[0] == "") range.L = long.MinValue; else range.L = long.Parse(g[0]);
            if (g[1] == null || g[1] == "") range.R = long.MinValue; else range.R = long.Parse(g[1]);
            return range;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(L, R);
        }
    }
}

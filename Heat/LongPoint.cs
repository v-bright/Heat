using System;
using System.Collections.Generic;
using System.Globalization;

namespace Heat
{
    [Serializable]
    public struct LongPoint
    {
        public static readonly LongPoint Empty = new LongPoint();

        public LongPoint(long x, long y) : this()
        {
            X = x;
            Y = y;
        }

        public LongPoint(LongSize sz) : this()
        {
            X = sz.Width;
            Y = sz.Height;
        }

        public bool IsEmpty
        {
            get { return X == 0 && Y == 0; }
        }

        public long X { get; set; }

        public long Y { get; set; }

        public static explicit operator LongSize(LongPoint p)
        {
            return new LongSize(p.X, p.Y);
        }

        public static LongPoint operator +(LongPoint pt, LongSize sz)
        {
            return Add(pt, sz);
        }

        public static LongPoint operator -(LongPoint pt, LongSize sz)
        {
            return Subtract(pt, sz);
        }

        public static bool operator ==(LongPoint left, LongPoint right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(LongPoint left, LongPoint right)
        {
            return !(left == right);
        }

        public static LongPoint Add(LongPoint pt, LongSize sz)
        {
            return new LongPoint(pt.X + sz.Width, pt.Y + sz.Height);
        }

        public static LongPoint Subtract(LongPoint pt, LongSize sz)
        {
            return new LongPoint(pt.X - sz.Width, pt.Y - sz.Height);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LongPoint))
                return false;
            var comp = (LongPoint) obj;
            return comp.X == X && comp.Y == Y;
        }

        public override int GetHashCode()
        {
            return (int) (X ^ Y);
        }

        public void Offset(long dx, long dy)
        {
            X += dx;
            Y += dy;
        }

        public void Offset(LongPoint p)
        {
            Offset(p.X, p.Y);
        }

        public void OffsetNegative(LongPoint p)
        {
            Offset(-p.X, -p.Y);
        }

        public override string ToString()
        {
            return "{ X = " + X.ToString(CultureInfo.CurrentCulture) + ", Y = " +
                   Y.ToString(CultureInfo.CurrentCulture) + " }";
        }
    }
}
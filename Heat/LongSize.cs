using System.Globalization;

namespace Heat
{
    public struct LongSize
    {
        public static readonly LongSize Empty = new LongSize();

        public LongSize(LongPoint pt)
        {
            Width = pt.X;
            Height = pt.Y;
        }

        public LongSize(long width, long height)
        {
            Width = width;
            Height = height;
        }

        public static LongSize operator +(LongSize sz1, LongSize sz2)
        {
            return Add(sz1, sz2);
        }

        public static LongSize operator -(LongSize sz1, LongSize sz2)
        {
            return Subtract(sz1, sz2);
        }

        public static bool operator ==(LongSize sz1, LongSize sz2)
        {
            return sz1.Width == sz2.Width && sz1.Height == sz2.Height;
        }

        public static bool operator !=(LongSize sz1, LongSize sz2)
        {
            return !(sz1 == sz2);
        }

        public static explicit operator LongPoint(LongSize size)
        {
            return new LongPoint(size.Width, size.Height);
        }

        public bool IsEmpty => Width == 0 && Height == 0;

        public long Width { get; set; }

        public long Height { get; set; }

        public static LongSize Add(LongSize sz1, LongSize sz2)
        {
            return new LongSize(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
        }

        public static LongSize Subtract(LongSize sz1, LongSize sz2)
        {
            return new LongSize(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LongSize))
                return false;

            var comp = (LongSize) obj;

            return comp.Width == Width && comp.Height == Height;
        }

        public override int GetHashCode()
        {
            if (IsEmpty)
                return 0;
            return Width.GetHashCode() ^ Height.GetHashCode();
        }

        public override string ToString()
        {
            return "{ Width = " + Width.ToString(CultureInfo.CurrentCulture) + ", Height = " +
                   Height.ToString(CultureInfo.CurrentCulture) + " }";
        }
    }
}
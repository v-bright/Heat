using System;
using System.Drawing;
using System.Globalization;

namespace Heat
{
    public struct RectLatLng
    {
        public static readonly RectLatLng Empty;

        public RectLatLng(PointLatLng nw, PointLatLng se)
        {
            Lng = nw.Lng;
            Lat = nw.Lat;
            WidthLng = se.Lng - nw.Lng;
            HeightLat = nw.Lat - se.Lat;
            IsEmpty = false;
        }

        public RectLatLng(double leftLng, double topLat, double rightLng, double bottomLat)
        {
            Lng = leftLng;
            Lat = topLat;
            WidthLng = rightLng - leftLng;
            HeightLat = topLat - bottomLat;
            IsEmpty = false;
        }

        public PointLatLng LocationTopLeft
        {
            get => new PointLatLng(Lat, Lng);
            set
            {
                Lng = value.Lng;
                Lat = value.Lat;
            }
        }

        public PointLatLng LocationRightBottom
        {
            get
            {
                var ret = new PointLatLng(Lat, Lng);
                ret.Offset(HeightLat, WidthLng);
                return ret;
            }
        }

        public PointLatLng LocationMiddle
        {
            get
            {
                var ret = new PointLatLng(Lat, Lng);
                ret.Offset(HeightLat / 2, WidthLng / 2);
                return ret;
            }
        }

        public SizeLatLng Size
        {
            get => new SizeLatLng(HeightLat, WidthLng);
            set
            {
                WidthLng = value.WidthLng;
                HeightLat = value.HeightLat;
            }
        }

        public double Lng { get; set; }

        public double Lat { get; set; }

        public double WidthLng { get; set; }

        public double HeightLat { get; set; }

        public double Left => Lng;

        public double Top => Lat;

        public double Right => Lng + WidthLng;

        public double Bottom => Lat - HeightLat;

        public bool IsEmpty { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is RectLatLng))
                return false;
            var ef = (RectLatLng) obj;
            return ef.Lng == Lng && ef.Lat == Lat && ef.WidthLng == WidthLng && ef.HeightLat == HeightLat;
        }

        public static bool operator ==(RectLatLng left, RectLatLng right)
        {
            return left.Lng == right.Lng && left.Lat == right.Lat && left.WidthLng == right.WidthLng &&
                   left.HeightLat == right.HeightLat;
        }

        public static bool operator !=(RectLatLng left, RectLatLng right)
        {
            return !(left == right);
        }

        public bool Contains(double lat, double lng)
        {
            return Lng <= lng && lng < Lng + WidthLng && Lat >= lat && lat > Lat - HeightLat;
        }

        public bool Contains(PointLatLng pt)
        {
            return Contains(pt.Lat, pt.Lng);
        }

        public bool Contains(RectLatLng rect)
        {
            return Lng <= rect.Lng && rect.Lng + rect.WidthLng <= Lng + WidthLng && Lat >= rect.Lat &&
                   rect.Lat - rect.HeightLat >= Lat - HeightLat;
        }

        public override int GetHashCode()
        {
            if (IsEmpty)
                return 0;
            return Lng.GetHashCode() ^ Lat.GetHashCode() ^ WidthLng.GetHashCode() ^ HeightLat.GetHashCode();
        }

        public void Inflate(double lat, double lng)
        {
            Lng -= lng;
            Lat += lat;
            WidthLng += 2d * lng;
            HeightLat += 2d * lat;
        }

        public void Inflate(SizeLatLng size)
        {
            Inflate(size.HeightLat, size.WidthLng);
        }

        public static RectLatLng Inflate(RectLatLng rect, double lat, double lng)
        {
            var ef = rect;
            ef.Inflate(lat, lng);
            return ef;
        }

        public void Intersect(RectLatLng rect)
        {
            var ef = Intersect(rect, this);
            Lng = ef.Lng;
            Lat = ef.Lat;
            WidthLng = ef.WidthLng;
            HeightLat = ef.HeightLat;
        }

        public static RectLatLng Intersect(RectLatLng a, RectLatLng b)
        {
            var lng = Math.Max(a.Lng, b.Lng);
            var num2 = Math.Min(a.Lng + a.WidthLng, b.Lng + b.WidthLng);

            var lat = Math.Max(a.Lat, b.Lat);
            var num4 = Math.Min(a.Lat + a.HeightLat, b.Lat + b.HeightLat);

            if (num2 >= lng && num4 >= lat)
                return new RectLatLng(lat, lng, num2, num4);
            return Empty;
        }

        public bool IntersectsWith(RectLatLng a)
        {
            return Left < a.Right && Top > a.Bottom && Right > a.Left && Bottom < a.Top;
        }

        public static RectLatLng Union(RectLatLng a, RectLatLng b)
        {
            return new RectLatLng(
                Math.Min(a.Left, b.Left),
                Math.Max(a.Top, b.Top),
                Math.Max(a.Right, b.Right),
                Math.Min(a.Bottom, b.Bottom));
        }

        public void Offset(PointLatLng pos)
        {
            Offset(pos.Lat, pos.Lng);
        }

        public void Offset(double lat, double lng)
        {
            Lng += lng;
            Lat -= lat;
        }

        public override string ToString()
        {
            return "{ Lat= " + Lat.ToString(CultureInfo.CurrentCulture) + ", Lng = " +
                   Lng.ToString(CultureInfo.CurrentCulture) + ", WidthLng = " +
                   WidthLng.ToString(CultureInfo.CurrentCulture) + ", HeightLat = " +
                   HeightLat.ToString(CultureInfo.CurrentCulture) + " }";
        }

        public Rectangle ToRect(LongPoint nwPixel, int zoom)
        {
            var topLeft = MercatorProjection.Instance.FromLatLngToPixel(LocationTopLeft, zoom);
            var bottomRight = MercatorProjection.Instance.FromLatLngToPixel(LocationRightBottom, zoom);

            var translatedPos = new Point((int)(topLeft.X - nwPixel.X), (int)(topLeft.Y - nwPixel.Y));
            var translatedSize = new Size((int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));

            return new Rectangle(translatedPos, translatedSize);
        }

        static RectLatLng()
        {
            Empty = new RectLatLng();
        }
    }
}
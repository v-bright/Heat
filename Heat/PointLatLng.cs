using System;
using System.Globalization;

namespace Heat
{
    [Serializable]
    public struct PointLatLng
    {
        public static readonly PointLatLng Empty = new PointLatLng();

        public PointLatLng(double lat, double lng) : this()
        {
            Lat = lat;
            Lng = lng;
            IsEmpty = false;
        }

        public bool IsEmpty { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public static PointLatLng operator +(PointLatLng pt, SizeLatLng sz)
        {
            return Add(pt, sz);
        }

        public static PointLatLng operator -(PointLatLng pt, SizeLatLng sz)
        {
            return Subtract(pt, sz);
        }

        public static SizeLatLng operator -(PointLatLng pt1, PointLatLng pt2)
        {
            return new SizeLatLng(pt1.Lat - pt2.Lat, pt2.Lng - pt1.Lng);
        }

        public static bool operator ==(PointLatLng left, PointLatLng right)
        {
            return left.Lng == right.Lng && left.Lat == right.Lat;
        }

        public static bool operator !=(PointLatLng left, PointLatLng right)
        {
            return !(left == right);
        }

        public static PointLatLng Add(PointLatLng pt, SizeLatLng sz)
        {
            return new PointLatLng(pt.Lat - sz.HeightLat, pt.Lng + sz.WidthLng);
        }

        public static PointLatLng Subtract(PointLatLng pt, SizeLatLng sz)
        {
            return new PointLatLng(pt.Lat + sz.HeightLat, pt.Lng - sz.WidthLng);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PointLatLng))
                return false;
            var tf = (PointLatLng) obj;
            return tf.Lng == Lng && tf.Lat == Lat && tf.GetType() == GetType();
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

        public override int GetHashCode()
        {
            return Lng.GetHashCode() ^ Lat.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{ Lat = {0}, Lng = {1} }}", Lat, Lng);
        }
    }
}
using System;

namespace Heat
{
    public class MercatorProjection : PureProjection
    {
        private const double MinLatitude = -85.05112878;
        private const double MaxLatitude = 85.05112878;
        private const double MinLongitude = -180;
        private const double MaxLongitude = 180;
        public static readonly MercatorProjection Instance = new MercatorProjection();

        public override RectLatLng Bounds
        {
            get { return new RectLatLng(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude); }
        }


        public override LongSize TileSize
        {
            get { return new LongSize(Heatmap.TileUnit, Heatmap.TileUnit); }
        }

        public override double Axis
        {
            get { return 6378137; }
        }

        public override double Flattening
        {
            get { return 1.0 / 298.257223563; }
        }

        public override LongPoint FromLatLngToPixel(double lat, double lng, int zoom)
        {
            var ret = LongPoint.Empty;

            lat = Clip(lat, MinLatitude, MaxLatitude);
            lng = Clip(lng, MinLongitude, MaxLongitude);

            var x = (lng + 180) / 360;
            var sinLatitude = Math.Sin(lat * Math.PI / 180);
            var y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            var s = GetTileMatrixSizePixel(zoom);
            var mapSizeX = s.Width;
            var mapSizeY = s.Height;

            ret.X = (long) Clip(x * mapSizeX + 0.5, 0, mapSizeX - 1);
            ret.Y = (long) Clip(y * mapSizeY + 0.5, 0, mapSizeY - 1);

            return ret;
        }

        public override PointLatLng FromPixelToLatLng(long x, long y, int zoom)
        {
            var ret = PointLatLng.Empty;

            var s = GetTileMatrixSizePixel(zoom);
            double mapSizeX = s.Width;
            double mapSizeY = s.Height;

            var xx = Clip(x, 0, mapSizeX - 1) / mapSizeX - 0.5;
            var yy = 0.5 - Clip(y, 0, mapSizeY - 1) / mapSizeY;

            ret.Lat = 90 - 360 * Math.Atan(Math.Exp(-yy * 2 * Math.PI)) / Math.PI;
            ret.Lng = 360 * xx;

            return ret;
        }

        public override LongSize GetTileMatrixMinXy(int zoom)
        {
            return new LongSize(0, 0);
        }

        public override LongSize GetTileMatrixMaxXy(int zoom)
        {
            long xy = 1 << zoom;
            return new LongSize(xy - 1, xy - 1);
        }
    }
}
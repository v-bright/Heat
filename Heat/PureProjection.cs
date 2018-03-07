using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Heat
{
    public abstract class PureProjection
    {
        private readonly List<Dictionary<PointLatLng, LongPoint>> _fromLatLngToPixelCache =
            new List<Dictionary<PointLatLng, LongPoint>>(33);

        private readonly List<Dictionary<LongPoint, PointLatLng>> _fromPixelToLatLngCache =
            new List<Dictionary<LongPoint, PointLatLng>>(33);

        protected PureProjection()
        {
            for (var i = 0; i < _fromLatLngToPixelCache.Capacity; i++)
            {
                _fromLatLngToPixelCache.Add(new Dictionary<PointLatLng, LongPoint>());
                _fromPixelToLatLngCache.Add(new Dictionary<LongPoint, PointLatLng>());
            }
        }

        public abstract LongSize TileSize { get; }

        public abstract double Axis { get; }

        public abstract double Flattening { get; }

        public virtual RectLatLng Bounds
        {
            get { return new RectLatLng(-180, 90, 180, -90); }
        }

        public abstract LongPoint FromLatLngToPixel(double lat, double lng, int zoom);

        public abstract PointLatLng FromPixelToLatLng(long x, long y, int zoom);

        public LongPoint FromLatLngToPixel(PointLatLng p, int zoom)
        {
            return FromLatLngToPixel(p, zoom, false);
        }

        public LongPoint FromLatLngToPixel(PointLatLng p, int zoom, bool useCache)
        {
            if (!useCache)
                return FromLatLngToPixel(p.Lat, p.Lng, zoom);

            LongPoint ret;

            if (_fromLatLngToPixelCache[zoom].TryGetValue(p, out ret))
                return ret;

            ret = FromLatLngToPixel(p.Lat, p.Lng, zoom);
            _fromLatLngToPixelCache[zoom].Add(p, ret);

            // for reverse cache
            if (!_fromPixelToLatLngCache[zoom].ContainsKey(ret))
                _fromPixelToLatLngCache[zoom].Add(ret, p);

            Debug.WriteLine("FromLatLngToPixelCache[" + zoom + "] added " + p + " with " + ret);
            return ret;
        }

        public PointLatLng FromPixelToLatLng(LongPoint p, int zoom)
        {
            return FromPixelToLatLng(p, zoom, false);
        }

        public PointLatLng FromPixelToLatLng(LongPoint p, int zoom, bool useCache)
        {
            if (!useCache)
                return FromPixelToLatLng(p.X, p.Y, zoom);

            PointLatLng ret;

            if (_fromPixelToLatLngCache[zoom].TryGetValue(p, out ret))
                return ret;

            ret = FromPixelToLatLng(p.X, p.Y, zoom);
            _fromPixelToLatLngCache[zoom].Add(p, ret);

            // for reverse cache
            if (!_fromLatLngToPixelCache[zoom].ContainsKey(ret))
                _fromLatLngToPixelCache[zoom].Add(ret, p);

            Debug.WriteLine("FromPixelToLatLngCache[" + zoom + "] added " + p + " with " + ret);
            return ret;
        }

        public virtual LongPoint FromPixelToTileXy(LongPoint p)
        {
            return new LongPoint(p.X / TileSize.Width, p.Y / TileSize.Height);
        }

        public virtual LongPoint FromTileXyToPixel(LongPoint p)
        {
            return new LongPoint(p.X * TileSize.Width, p.Y * TileSize.Height);
        }

        public abstract LongSize GetTileMatrixMinXy(int zoom);

        public abstract LongSize GetTileMatrixMaxXy(int zoom);

        public virtual LongSize GetTileMatrixSizeXy(int zoom)
        {
            var sMin = GetTileMatrixMinXy(zoom);
            var sMax = GetTileMatrixMaxXy(zoom);

            return new LongSize(sMax.Width - sMin.Width + 1, sMax.Height - sMin.Height + 1);
        }

        public virtual LongSize GetTileMatrixSizePixel(int zoom)
        {
            var s = GetTileMatrixSizeXy(zoom);
            return new LongSize(s.Width * TileSize.Width, s.Height * TileSize.Height);
        }

        protected static double Clip(double n, double minValue, double maxValue)
        {
            return Math.Min(Math.Max(n, minValue), maxValue);
        }
    }
}
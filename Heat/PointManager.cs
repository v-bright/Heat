using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Heat
{
    public class PointManager
    {
        #region Static Methods

        public static LongPoint AdjustMapPixelsToTilePixels(LongPoint tileXyPoint, LongPoint mapPixelPoint)
        {
            return new LongPoint(mapPixelPoint.X - tileXyPoint.X * Heatmap.TileUnit,
                mapPixelPoint.Y - tileXyPoint.Y * Heatmap.TileUnit);
        }

        #endregion

        #region Fields

        private readonly IEnumerable<PointLatLng> _pointList;
        private readonly LongPoint _imageDimensions;

        #endregion

        #region Constructors

        public PointManager(IEnumerable<PointLatLng> points) : this(points, Heatmap.MaxFreeTierImageDimensions)
        {
        }

        public PointManager(IEnumerable<PointLatLng> points, LongPoint imageDimensions)
        {
            _imageDimensions = imageDimensions;
            _pointList = points;
        }

        #endregion

        #region Methods

        public RectLatLng GetBoundsForPoints(PointLatLng[] pointList)
        {
            double minLng = -180;
            double maxLng = 180;
            double maxLat = -90;
            double minLat = 90;

            foreach (var point in pointList)
            {
                maxLng = point.Lng < maxLng ? point.Lng : maxLng;
                minLng = point.Lng > minLng ? point.Lng : minLng;
                minLat = point.Lat < minLat ? point.Lat : minLat;
                maxLat = point.Lat > maxLat ? point.Lat : maxLat;
            }

            var se = new PointLatLng(minLat, minLng);
            var nw = new PointLatLng(maxLat, maxLng);

            return new RectLatLng(nw, se);
        }

        public int GetBoundsZoomLevel(RectLatLng rect)
        {
            var latFraction = (LatRad(rect.LocationTopLeft.Lat) - LatRad(rect.LocationRightBottom.Lat)) / Math.PI;

            var lngFraction = (rect.WidthLng < 0 ? rect.WidthLng + 360 : rect.WidthLng) / 360;

            var latZoom = Math.Min(Zoom((int) _imageDimensions.Y, Heatmap.TileUnit, latFraction), Heatmap.MaxZoom);
            var lngZoom = Math.Min(Zoom((int) _imageDimensions.X, Heatmap.TileUnit, lngFraction), Heatmap.MaxZoom);

            return (int) Math.Min(latZoom, lngZoom);
        }

        protected PointLatLng[] GetList(LongPoint tlb, LongPoint lrb, int zoom)
        {
            var ptlb = MercatorProjection.Instance.FromPixelToLatLng(tlb, zoom);
            var plrb = MercatorProjection.Instance.FromPixelToLatLng(lrb, zoom);

            return _pointList.Where(point =>
                    point.Lat <= ptlb.Lat && point.Lng >= ptlb.Lng && point.Lat >= plrb.Lat && point.Lng <= plrb.Lng)
                .ToArray();
        }

        public LongPoint[] GetPointsForTile(int x, int y, Bitmap dot, int zoom)
        {
            var points = new List<LongPoint>();

            //Top left Bounds
            var tlb = MercatorProjection.Instance.FromTileXyToPixel(new LongPoint(x, y));

            var maxTileSize = new LongSize(Heatmap.TileUnit, Heatmap.TileUnit);
            //Lower right bounds
            var lrb = new LongPoint(tlb.X + maxTileSize.Width + dot.Width, tlb.Y + maxTileSize.Height + dot.Width);

            //Pad the top left bounds
            tlb = new LongPoint(tlb.X - dot.Width, tlb.Y - dot.Height);

            //Go through the list and convert the points to pixel coordinates
            foreach (var llPoint in GetList(tlb, lrb, zoom))
            {
                var pixelCoordinate = MercatorProjection.Instance.FromLatLngToPixel(llPoint.Lat, llPoint.Lng, zoom);

                MercatorProjection.Instance.FromPixelToTileXy(pixelCoordinate);

                //Adjust the point to the specific tile
                var adjustedPoint = AdjustMapPixelsToTilePixels(new LongPoint(x, y), pixelCoordinate);

                //Add the point to the list
                points.Add(adjustedPoint);
            }

            return points.ToArray();
        }

        public LongPoint GetTileOffsets(LongPoint startTile, RectLatLng heatmapBounds, int zoom)
        {
            var centerPixel = MercatorProjection.Instance.FromLatLngToPixel(heatmapBounds.LocationMiddle, zoom);

            var startTilePixel = MercatorProjection.Instance.FromTileXyToPixel(startTile);
            var tileCenter = new LongPoint(startTilePixel.X + _imageDimensions.X / 2,
                startTilePixel.Y + _imageDimensions.Y / 2);

            var offset = new LongPoint(tileCenter.X - centerPixel.X, tileCenter.Y - centerPixel.Y);

            return offset;
        }

        public LongPoint[] GetTileProjectionsForLatLngBounds(RectLatLng bounds, int zoom)
        {
            var neProj =
                MercatorProjection.Instance.FromPixelToTileXy(
                    MercatorProjection.Instance.FromLatLngToPixel(bounds.LocationTopLeft, zoom));
            var swProj =
                MercatorProjection.Instance.FromPixelToTileXy(
                    MercatorProjection.Instance.FromLatLngToPixel(bounds.LocationRightBottom, zoom));

            return new[] {neProj, swProj};
        }

        public LongPoint GetTopLeftPixelForLatLngBounds(RectLatLng bounds, long xOffset, long yOffset, int zoom)
        {
            var nwPixel = MercatorProjection.Instance.FromLatLngToPixel(bounds.LocationTopLeft, zoom);

            return new LongPoint(nwPixel.X - nwPixel.X % Heatmap.TileUnit - xOffset,
                nwPixel.Y - nwPixel.Y % Heatmap.TileUnit - yOffset);
        }

        private static double LatRad(double lat)
        {
            var sin = Math.Sin(lat * Math.PI / 180);
            var radX2 = Math.Log((1 + sin) / (1 - sin)) / 2;
            return Math.Max(Math.Min(radX2, Math.PI), -Math.PI) / 2;
        }

        private static long Zoom(int mapPx, int worldPx, double fraction)
        {
            return (long) Math.Floor(Math.Log((float) mapPx / worldPx / fraction) / 0.6931471805599453);
        }

        #endregion
    }
}
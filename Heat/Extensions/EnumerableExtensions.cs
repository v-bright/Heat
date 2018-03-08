using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Heat.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<Point> ToPoints(this IEnumerable<PointLatLng> source, LongPoint nwPixel, int zoom)
        {
            return source.Select(point =>
            {
                var pixelPos = MercatorProjection.Instance.FromLatLngToPixel(point, zoom);
                return new Point((int)(pixelPos.X - nwPixel.X), (int)(pixelPos.Y - nwPixel.Y));
            });
        }
    }
}
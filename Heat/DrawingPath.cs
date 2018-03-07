using System.Collections.Generic;
using System.Drawing;

namespace Heat
{
    public class DrawingPath
    {
        private readonly IEnumerable<PointLatLng> _pathCoordinates;
        private int? _pathWeight;
        private Color? _pathColor;
        private Color? _pathFillColor;
        private readonly bool _geoDesic;

        public DrawingPath(IEnumerable<PointLatLng> pathCoordinates)
            : this(pathCoordinates, null, null, null, false)
        {

        }

        public DrawingPath(IEnumerable<PointLatLng> pathCoordinates, int? pathWeight)
            : this(pathCoordinates, pathWeight, null, null, false)
        {

        }

        public DrawingPath(IEnumerable<PointLatLng> pathCoordinates, Color? pathColor)
            : this(pathCoordinates, null, pathColor, null, false)
        {

        }

        public DrawingPath(IEnumerable<PointLatLng> pathCoordinates, int? pathWeight, Color? pathColor)
            : this(pathCoordinates, pathWeight, pathColor, null, false)
        {

        }

        public DrawingPath(IEnumerable<PointLatLng> pathCoordinates, int? pathWeight, Color? pathColor, Color? pathFillColor)
            :this(pathCoordinates, pathWeight, pathColor, pathFillColor, false)
        {
            
        }
        public DrawingPath(IEnumerable<PointLatLng> pathCoordinates, int? pathWeight, Color? pathColor, Color? pathFillColor, bool geoDesic)
        {
            _pathCoordinates = pathCoordinates;
            _pathWeight = pathWeight;
            _pathColor = pathColor;
            _pathFillColor = pathFillColor;
            _geoDesic = geoDesic;
        }

        public override string ToString()
        {
            var pathColorString = _pathColor.HasValue ? "color:" + HexConverter(_pathColor.Value) + "|" : string.Empty;
            var pathFillColorString = _pathFillColor.HasValue ? "fillcolor:" + HexConverter(_pathFillColor.Value) + "|" : string.Empty;
            var geoDesicString = _geoDesic ? "geodesic:true|" : string.Empty;
            var pathWeightString = _pathWeight.HasValue ? "weight:" + _pathWeight.Value + "|" : string.Empty;
            var pathCoordinateString = string.Empty;

            foreach (var coordinate in _pathCoordinates)
            {
                //Avoid epsilon values in these strings, it messes with the image retrieval
                var lat = coordinate.Lat.ToString("0." + new string('#', 339));
                var lng = coordinate.Lng.ToString("0." + new string('#', 339));

                pathCoordinateString += lat + "," + lng + "|";
            }

            pathCoordinateString = pathCoordinateString.Remove(pathCoordinateString.Length - 1, 1);

            return "path="+pathColorString+pathFillColorString+pathWeightString+geoDesicString+pathCoordinateString;
        }

        private static string HexConverter(Color c)
        {
            return "0x" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2") + c.A.ToString("X2");
        }
    }
}
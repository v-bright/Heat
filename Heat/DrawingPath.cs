using System.Collections.Generic;
using System.Drawing;
using Heat.Extensions;

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
            var pathColorString = _pathColor.HasValue ? "color:" + _pathColor.Value.ToHexValue() + "|" : string.Empty;
            var pathFillColorString = _pathFillColor.HasValue ? "fillcolor:" + _pathFillColor.Value.ToHexValue() + "|" : string.Empty;
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

            if (pathCoordinateString[pathCoordinateString.Length-1] == '|')
                pathCoordinateString = pathCoordinateString.Remove(pathCoordinateString.Length - 1, 1);

            var query = "path=" + pathColorString + pathFillColorString + pathWeightString + geoDesicString + pathCoordinateString;

            if (query[query.Length - 1] == '|')
                query = query.Remove(query.Length - 1, 1);

            return query;
        }
    }
}
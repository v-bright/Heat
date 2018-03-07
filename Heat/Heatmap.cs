using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Heat.ColorSchemes;
using Heat.Dots;

namespace Heat
{
    public class Heatmap
    {
        #region Fields

        private readonly PointManager _pointManager;
        private readonly PointLatLng _mapCenter;
        private readonly LongPoint _northWestPixel;
        private readonly LongPoint _startTile;
        private readonly LongPoint _endTile;
        private readonly LongPoint _tileOffsets;
        private readonly LongPoint _imageDimensions;
        private readonly int _zoom;

        #endregion

        #region Constants

        public const int MaxZoom = 31;
        public const int TileUnit = 256;

        #endregion

        #region Static Fields

        public static readonly LongPoint MaxFreeTierImageDimensions = new LongPoint(640, 640);
        public static readonly LongPoint MaxPremiumTierImageDimensions = new LongPoint(2048, 2048);
        private static readonly Dictionary<string, Bitmap> _colorSchemeList = ColorScheme.GetBitmaps();
        private static readonly Dictionary<string, Bitmap> _dotsList = Dot.GetDots();

        #endregion

        #region Static Methods

        public static string[] AvailableColorSchemes()
        {
            return _colorSchemeList.Keys.Select(key => key.Replace("." + ImageFormat.Png.ToString().ToLower(), ""))
                .ToArray();
        }

        public static Bitmap GetColorScheme(string schemeName)
        {
            if (!_colorSchemeList.ContainsKey(schemeName + "." + ImageFormat.Png.ToString().ToLower()))
                throw new Exception("Color scheme '" + schemeName + " could not be found");
            return _colorSchemeList[schemeName + "." + ImageFormat.Png.ToString().ToLower()];
        }

        private static Bitmap GetDot(int zoom)
        {
            var dot = _dotsList["dot" + zoom + "." + ImageFormat.Png.ToString().ToLower()];
            var bmp = new Bitmap(dot.Width, dot.Height, PixelFormat.Format24bppRgb);

            using (var graphics = Graphics.FromImage(bmp))
            {
                const float opacity = 1.1f;

                var colorMatrix = new ColorMatrix
                {
                    Matrix00 = opacity,
                    Matrix11 = opacity,
                    Matrix22 = opacity
                };

                // Create an ImageAttributes object and set its color matrix.
                var imageAtt = new ImageAttributes();
                imageAtt.SetColorMatrix(
                    colorMatrix,
                    ColorMatrixFlag.Default,
                    ColorAdjustType.Bitmap);

                var imageWidth = dot.Width;
                var imageHeight = dot.Height;

                graphics.DrawImage(
                    dot,
                    new Rectangle(0, 0, imageWidth, imageHeight), // destination rectangle
                    0.0f, // source rectangle x 
                    0.0f, // source rectangle y
                    imageWidth, // source rectangle width
                    imageHeight, // source rectangle height
                    GraphicsUnit.Pixel,
                    imageAtt);
            }

            return bmp;
        }

        public static Bitmap GetTile(PointManager pm, string colorScheme, int zoom, int x, int y,
            bool changeOpacityWithZoom, int defaultOpacity)
        {
            if (colorScheme == string.Empty)
                throw new Exception("A color scheme is required");
            if (pm == null)
                throw new Exception("No point manager has been specified");
            return Tile.Generate(GetColorScheme(colorScheme), GetDot(zoom), zoom, x, y,
                pm.GetPointsForTile(x, y, GetDot(zoom), zoom), changeOpacityWithZoom, defaultOpacity);
        }

        public static Bitmap GetTile(PointManager pm, string colorScheme, int zoom, int x, int y)
        {
            return GetTile(pm, colorScheme, zoom, x, y, true, 0);
        }

        private static string Sign(string url, string keyString)
        {
            // converting key to bytes will throw an exception, need to replace '-' and '_' characters first.
            var usablePrivateKey = keyString.Replace("-", "+").Replace("_", "/");
            var privateKeyBytes = Convert.FromBase64String(usablePrivateKey);

            var uri = new Uri(url);
            var encodedPathAndQueryBytes = Encoding.ASCII.GetBytes(uri.LocalPath + uri.Query);

            // compute the hash
            using (var algorithm = new HMACSHA1(privateKeyBytes))
            {
                var hash = algorithm.ComputeHash(encodedPathAndQueryBytes);

                // convert the bytes to string and make url-safe by replacing '+' and '/' characters
                var signature = Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_");

                // Add the signature to the existing URI.
                return uri.Scheme + "://" + uri.Host + uri.LocalPath + uri.Query + "&signature=" + signature;
            }
        }

        #endregion

        #region Constructors

        public Heatmap(IEnumerable<PointLatLng> points)
            : this(points, points, MaxFreeTierImageDimensions)
        {
        }

        public Heatmap(IEnumerable<PointLatLng> points, LongPoint imageDimensions)
            : this(points, points, imageDimensions)
        {
        }

        public Heatmap(IEnumerable<PointLatLng> points, RectLatLng viewPort)
            : this(points, new[] {viewPort.LocationTopLeft, viewPort.LocationRightBottom}, MaxFreeTierImageDimensions)
        {
        }

        public Heatmap(IEnumerable<PointLatLng> points, RectLatLng viewPort, LongPoint imageDimensions)
            : this(points, new[] {viewPort.LocationTopLeft, viewPort.LocationRightBottom}, imageDimensions)
        {
        }

        public Heatmap(IEnumerable<PointLatLng> points, IEnumerable<PointLatLng> viewPort)
            : this(points, viewPort, MaxFreeTierImageDimensions)
        {
        }

        public Heatmap(IEnumerable<PointLatLng> points, IEnumerable<PointLatLng> viewPort,
            LongPoint imageDimensions)
        {
            _pointManager = new PointManager(points, imageDimensions);
            _imageDimensions = imageDimensions;

            // Create northwest and southeast boundaries. If these haven't been specified by the user, find bounds that fit all given points
            var bounds = _pointManager.GetBoundsForPoints(viewPort.ToArray());

            // Get an appropriate zoom level, given the heatmap bounds
            _zoom = _pointManager.GetBoundsZoomLevel(bounds);

            // Tiles in google maps are always 256x256. Work out which tiles we're drawing on
            var tileProjections = _pointManager.GetTileProjectionsForLatLngBounds(bounds, _zoom);

            _startTile = tileProjections[0];
            _endTile = tileProjections[1];

            // Account for cases where the dimensions of our heatmap are less than 2048x2048
            _tileOffsets = _pointManager.GetTileOffsets(_startTile, bounds, _zoom);

            _mapCenter = bounds.LocationMiddle;

            _northWestPixel =
                _pointManager.GetTopLeftPixelForLatLngBounds(bounds, _tileOffsets.X, _tileOffsets.Y, _zoom);
        }

        #endregion

        #region Methods

        public void DrawHeatmap(Bitmap sourceBitmap)
        {
            using (var graphics = Graphics.FromImage(sourceBitmap))
            {
                for (var x = _startTile.X; x <= _endTile.X; x++)
                for (var y = _startTile.Y; y <= _endTile.Y; y++)
                {
                    var tempImage = GetTile(_pointManager, "valerie", _zoom, (int) x, (int) y);
                    var xDiff = x - _startTile.X;
                    var yDiff = y - _startTile.Y;

                    graphics.DrawImage(tempImage,
                        new PointF((float) _tileOffsets.X + xDiff * TileUnit,
                            (float) _tileOffsets.Y + yDiff * TileUnit));
                }
            }
        }

        public void DrawImage(Bitmap sourceBitmap, Bitmap overlay, RectLatLng overlayBounds)
        {
            using (var graphics = Graphics.FromImage(sourceBitmap))
            {
                var rect = overlayBounds.ToRect(_northWestPixel, _zoom);
                graphics.DrawImage(overlay, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        public void DrawImage(Bitmap sourceBitmap, Bitmap overlay, RectLatLng overlayBounds, float rotation)
        {
            using (var graphics = Graphics.FromImage(sourceBitmap))
            {
                var rect = overlayBounds.ToRect(_northWestPixel, _zoom);

                var moveX = rect.Width / 2f + rect.X;
                var moveY = rect.Height / 2f + rect.Y;
                graphics.TranslateTransform(moveX, moveY);
                graphics.RotateTransform(rotation);
                graphics.TranslateTransform(-moveX, -moveY);
                graphics.DrawImage(overlay, rect.X, rect.Y, rect.Width, rect.Height);
                graphics.ResetTransform();
            }
        }

        public void DrawPolygon(Bitmap sourceBitmap, Pen pen, IEnumerable<PointLatLng> drawingCoordinates)
        {
            var drawingPoints = drawingCoordinates.ToPoints(_northWestPixel, _zoom);

            using (var graphics = Graphics.FromImage(sourceBitmap))
            {
                graphics.DrawPolygon(pen, drawingPoints.ToArray());
            }
        }

        public void FillPolygon(Bitmap sourceBitmap, SolidBrush brush, IEnumerable<PointLatLng> drawingCoordinates)
        {
            var drawingPoints = drawingCoordinates.ToPoints(_northWestPixel, _zoom);

            using (var graphics = Graphics.FromImage(sourceBitmap))
            {
                graphics.FillPolygon(brush, drawingPoints.ToArray());
            }
        }

        public Task<Stream> GetMap()
        {
            if (_imageDimensions.X > MaxFreeTierImageDimensions.X || _imageDimensions.Y > MaxFreeTierImageDimensions.Y)
                throw new NotSupportedException("Images greater than " + MaxFreeTierImageDimensions.X + "x" +
                                                MaxFreeTierImageDimensions.Y +
                                                " require a valid client Id and secret.");

            //Avoid epsilon values in these strings, it messes with the image retrieval
            var lat = _mapCenter.Lat.ToString("0." + new string('#', 339));
            var lng = _mapCenter.Lng.ToString("0." + new string('#', 339));

            var path = "http://maps.googleapis.com/maps/api/staticmap" + "?center=" + lat + "," + lng +
                       "&zoom=" + _zoom + "&size=" + _imageDimensions.X + "x" + _imageDimensions.Y +
                       "&maptype=roadmap&sensor=false";

            using (var wc = new WebClient())
            {
                return wc.OpenReadTaskAsync(path);
            }
        }

        public Task<Stream> GetMap(string googleClientId, string googleSecretKey)
        {
            if (_imageDimensions.X > MaxPremiumTierImageDimensions.X ||
                _imageDimensions.Y > MaxPremiumTierImageDimensions.Y)
                throw new NotSupportedException("Images greater than " + MaxPremiumTierImageDimensions.X + "x" +
                                                MaxPremiumTierImageDimensions.Y + " are not supported.");

            //Avoid epsilon values in these strings, it messes with the image retrieval
            var lat = _mapCenter.Lat.ToString("0." + new string('#', 339));
            var lng = _mapCenter.Lng.ToString("0." + new string('#', 339));

            var path = "http://maps.googleapis.com/maps/api/staticmap" + "?center=" + lat + "," + lng +
                       "&zoom=" + _zoom + "&client=" + googleClientId + "&size=" + _imageDimensions.X + "x" +
                       _imageDimensions.Y + "&maptype=roadmap&sensor=false";

            path = Sign(path, googleSecretKey);

            using (var wc = new WebClient())
            {
                return wc.OpenReadTaskAsync(path);
            }
        }

        #endregion
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<Point> ToPoints(this IEnumerable<PointLatLng> source, LongPoint nwPixel, int zoom)
        {
            return source.Select(point =>
            {
                var pixelPos = MercatorProjection.Instance.FromLatLngToPixel(point, zoom);
                return new Point((int) (pixelPos.X - nwPixel.X), (int) (pixelPos.Y - nwPixel.Y));
            });
        }
    }
}
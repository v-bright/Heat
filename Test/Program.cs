using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Heat;

namespace Test
{
    internal class Program
    {
        private static IEnumerable<PointLatLng> _pointList;

        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            await DrawMap();
        }

        public static async Task DrawMap()
        {
            LoadPointsFromFile("SampleData.txt");

            var northWestCoordinate = new PointLatLng(50, -100);
            var southEastCoordinate = new PointLatLng(20, -70);

            var viewPort = new RectLatLng(northWestCoordinate, southEastCoordinate);

            var heatmap = new Heatmap(_pointList, viewPort, Heatmap.MaxFreeTierImageDimensions);

            using (var mapStream = await heatmap.GetMap())
            using (var sourceImage = new Bitmap(mapStream))
            using (var canvas = new Bitmap(sourceImage, sourceImage.Width, sourceImage.Height))
            {
                heatmap.DrawHeatmap(canvas);
                canvas.Save("Heatmap.png");
            }
        }

        public static void LoadPointsFromFile(string source)
        {
            var newPoints = new List<PointLatLng>();

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program), source);
            var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var item = reader.ReadLine().Split(',');
                newPoints.Add(new PointLatLng(double.Parse(item[1]), double.Parse(item[2])));
            }

            _pointList = newPoints.ToList();
        }
    }
}
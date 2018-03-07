using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Heat
{
    public class Tile
    {
        #region Static Fields

        private static readonly Dictionary<string, Bitmap> EmptyTile = new Dictionary<string, Bitmap>();
        private static readonly int[] ZoomOpacity = new Opacity().BuildZoomMapping();

        #endregion

        #region Static Methods

        public static Bitmap AddPoints(Bitmap tile, Bitmap dot, LongPoint[] points)
        {
            var blender = new ImageBlender();

            foreach (var point in points)
                //Blend each dot to the existing images
                blender.BlendImages(
                    tile, // Destination Image
                    (int) point.X + dot.Width, //Dest x
                    (int) point.Y + dot.Width, //Dest y
                    dot.Width, // Dest width
                    dot.Height, // Dest height
                    //If their is a weight then change the dot so it reflects that intensity
                    dot, // Src Image
                    0, // Src x
                    0, // Src y
                    ImageBlender.BlendOperation.BlendMultiply);
            return tile;
        }

        public static Bitmap Colorize(Bitmap tile, Bitmap colorScheme, int zoomOpacity)
        {
            for (var x = 0; x < tile.Width; x++)
            for (var y = 0; y < tile.Height; y++)
            {
                var tilePixelColor = tile.GetPixel(x, y);

                var colorSchemePixel = colorScheme.GetPixel(0, tilePixelColor.R);

                tile.SetPixel(x, y, colorSchemePixel);
            }
            return tile;
        }

        public static Bitmap Generate(Bitmap colorScheme, Bitmap dot, int zoom, int tileX, int tileY,
            LongPoint[] points, bool changeOpacityWithZoom, int defaultOpacity)
        {
            if (defaultOpacity < Opacity.Transparent || defaultOpacity > Opacity.Opaque)
                throw new Exception("The default opacity of '" + defaultOpacity + "' doesn't fall between '" +
                                    Opacity.Transparent + "' and '" + Opacity.Opaque + "'");

            //Translate tile to pixel coords.
            var x1 = tileX * Heatmap.TileUnit;
            var x2 = x1 + 255;
            var y1 = tileY * Heatmap.TileUnit;
            var y2 = y1 + 255;

            var extraPad = dot.Width * 2;

            //Expand bounds by one dot width.
            x1 = x1 - extraPad;
            x2 = x2 + extraPad;
            y1 = y1 - extraPad;
            y2 = y2 + extraPad;
            var expandedWidth = x2 - x1;
            var expandedHeight = y2 - y1;

            Bitmap tile;
            if (points.Length == 0)
            {
                tile = GetEmptyTile(colorScheme, changeOpacityWithZoom ? ZoomOpacity[zoom] : defaultOpacity);
            }
            else
            {
                tile = GetBlankImage(expandedHeight, expandedWidth);
                tile = AddPoints(tile, dot, points);
                tile = Trim(tile, dot);
                tile = Colorize(tile, colorScheme, changeOpacityWithZoom ? ZoomOpacity[zoom] : defaultOpacity);
            }
            return tile;
        }

        public static Bitmap GetBlankImage(int height, int width)
        {
            //Create a blank tile that is 32 bit and has an alpha
            var newImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(newImage);
            //Background must be white so the dots can blend
            graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, width, height));
            return newImage;
        }

        public static Bitmap GetEmptyTile(Bitmap colorScheme, int zoomOpacity)
        {
            //If we have already created the empty tile then return it
            if (EmptyTile.ContainsKey(colorScheme.GetHashCode() + "_" + zoomOpacity))
                return EmptyTile[colorScheme.GetHashCode() + "_" + zoomOpacity];

            //Create a blank tile that is 32 bit and has an alpha
            var tile = new Bitmap(Heatmap.TileUnit, Heatmap.TileUnit, PixelFormat.Format32bppArgb);

            var graphic = Graphics.FromImage(tile);

            //Get the first pixel of the color scheme, on the dark side 
            var colorSchemePixelColor = colorScheme.GetPixel(0, colorScheme.Height - 1);

            zoomOpacity = (int) (zoomOpacity / 255.0f * (colorSchemePixelColor.A / 255.0f) * 255.0f);

            var solidBrush = new SolidBrush(Color.FromArgb(zoomOpacity, colorSchemePixelColor.R,
                colorSchemePixelColor.G, colorSchemePixelColor.B));
            graphic.FillRectangle(solidBrush, 0, 0, Heatmap.TileUnit, Heatmap.TileUnit);
            graphic.Dispose();

            //Save the newly created empty tile
            //There is a empty tile for each scheme and zoom level
            //Double check it does not already exists
            if (!EmptyTile.ContainsKey(colorScheme.GetHashCode().ToString()))
                EmptyTile.Add(colorScheme.GetHashCode().ToString(), tile);

            return tile;
        }

        public static Bitmap Trim(Bitmap tile, Bitmap dot)
        {
            var croppedTile = new Bitmap(Heatmap.TileUnit, Heatmap.TileUnit, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(croppedTile);
            var adjPad = dot.Width + dot.Width / 2;

            g.DrawImage(
                tile, // Source Image
                new Rectangle(0, 0, Heatmap.TileUnit, Heatmap.TileUnit),
                adjPad, // source x, adjusted for padded amount
                adjPad, // source y, adjusted for padded amount
                Heatmap.TileUnit, //source width
                Heatmap.TileUnit, // source height
                GraphicsUnit.Pixel
            );
            return croppedTile;
        }

        #endregion
    }
}
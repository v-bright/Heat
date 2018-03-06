using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Heat.ColorSchemes
{
    public class ColorScheme
    {
        #region Static Fields
        private static readonly string[] Files = { "valerie.png" };
        #endregion

        #region Static Methods
        private static Bitmap GetBitmap(string name)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(ColorScheme), name);
            if (stream == null)
                throw new InvalidOperationException("Resource not found");
            return new Bitmap(stream);
        }

        public static Dictionary<string, Bitmap> GetBitmaps()
        {
            return Files.ToDictionary(x => x, x => GetBitmap(x));
        }
        #endregion
    }
}
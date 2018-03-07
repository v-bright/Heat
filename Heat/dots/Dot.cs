using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Heat.Dots
{
    public class Dot
    {
        #region Static Methods
        private static Bitmap GetBitmap(string name)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Dot), name);
            if (stream == null)
                throw new InvalidOperationException("Resource not found");
            return new Bitmap(stream);
        }

        public static Dictionary<string, Bitmap> GetDots()
        {
            return Enumerable.Range(0, 31)
                             .Select(x => "dot"+x+".png")
                             .ToDictionary(x => x, GetBitmap);
        }
        #endregion
    }
}
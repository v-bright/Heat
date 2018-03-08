using System.Drawing;

namespace Heat.Extensions
{
    public static class ColorExtensions
    {
        public static string ToHexValue(this Color c)
        {
            return "0x" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2") + c.A.ToString("X2");
        }
    }
}
using System.Drawing;
using Heat.Extensions;

namespace Heat.Styles
{
    public class MapStyleOptions
    {
        public Color? Hue { get; set; }
        public float? Lightness { get; set; }
        public float? Saturation { get; set; }
        public float? Gamma { get; set; }
        public bool InvertLightness { get; set; }
        public ElementVisibility? Visibility { get; set; }
        public Color? Color { get; set; }
        public int? Weight { get; set; }

        public override string ToString()
        {
            var hueString = Hue.HasValue ? "hue:" + Hue.Value.ToHexValue() + "|" : string.Empty;
            var lightnessString = Lightness.HasValue ? "lightness:" + Lightness.Value + "|" : string.Empty;
            var saturationString = Saturation.HasValue ? "saturation:" + Saturation.Value + "|" : string.Empty;
            var gammaString = Gamma.HasValue ? "gamma:" + Gamma.Value + "|" : string.Empty;
            var invertLightnessString = InvertLightness ? "invert_lightness:true|" : string.Empty;
            var colorString = Color.HasValue ? "color:" + Color.Value.ToHexValue() + "|" : string.Empty;
            var weightString = Weight.HasValue ? "weight:" + Weight.Value + "|" : string.Empty;

            var visibilityString = string.Empty;

            switch (Visibility)
            {
                case ElementVisibility.Off:
                    visibilityString = "visibility:off|";
                    break;
                case ElementVisibility.On:
                    visibilityString = "visibility:on|";
                    break;
                case ElementVisibility.Simplified:
                    visibilityString = "visibility:simplified|";
                    break;
            }

            var query = hueString + lightnessString + saturationString + gammaString + invertLightnessString +
                        colorString + weightString + visibilityString;

            if (query[query.Length - 1] == '|')
                query = query.Remove(query.Length - 1, 1);

            return query;
        }
    }
}
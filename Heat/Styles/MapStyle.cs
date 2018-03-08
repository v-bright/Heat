namespace Heat.Styles
{
    public class MapStyle
    {
        private readonly string _mapElement;
        private readonly string _mapFeature;
        private readonly MapStyleOptions _styleOptions;

        public MapStyle(string mapFeature, string mapElement, MapStyleOptions styleOptions)
        {
            _mapFeature = mapFeature;
            _mapElement = mapElement;
            _styleOptions = styleOptions;
        }

        public override string ToString()
        {
            var featureString = _mapFeature == null ? string.Empty : "feature:" + _mapFeature + "|";

            return "style=" + featureString + "element:" + _mapElement + "|" + _styleOptions;
        }
    }
}
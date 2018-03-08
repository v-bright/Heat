namespace Heat.Features
{
    public class MapFeature
    {
        public const string All = "all";
        public const string Water = "water";
        public static readonly AdministrativeFeature Administrative = new AdministrativeFeature();
        public static readonly LandscapeFeature Landscape = new LandscapeFeature();
        public static readonly PoiFeature Poi = new PoiFeature();
        public static readonly RoadFeature Road = new RoadFeature();
        public static readonly TransitFeature Transit = new TransitFeature();
    }
}
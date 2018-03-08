namespace Heat.Features
{
    public class LandscapeFeature : ILandscapeFeature
    {
        public string All { get { return "landscape"; } }
        public string ManMade { get { return "landscape.man_made"; } }
        public INaturalFeature Natural { get { return new NaturalFeature(); } }
    }
}
namespace Heat.Features
{
    public class NaturalFeature : INaturalFeature
    {
        public string All { get { return "landscape.natural"; } }
        public string LandCover { get { return "landscape.natural.landcover"; } }
        public string Terrain { get { return "landscape.natural.terrain"; } }
    }
}
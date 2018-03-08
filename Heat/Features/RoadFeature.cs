namespace Heat.Features
{
    public class RoadFeature : IRoadFeature
    {
        public string All { get { return "road"; } }
        public string Arterial { get { return "road.arterial"; } }
        public string Local { get { return "road.local"; } }
        public IHighwayFeature Highway { get { return new HighwayFeature(); } }
    }
}
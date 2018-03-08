namespace Heat.Features
{
    public class HighwayFeature : IHighwayFeature
    {
        public string All { get { return "road.highway"; } }
        public string ControlledAccess { get { return "road.highway.controlled_access"; } }
    }
}
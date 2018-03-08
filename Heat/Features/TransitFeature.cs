namespace Heat.Features
{
    public class TransitFeature : ITransitFeature
    {
        public string All { get { return "transit"; } }
        public string Line { get { return "transit.line"; } }
        public IStationFeature Station { get { return new StationFeature(); } }
    }
}
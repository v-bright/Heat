namespace Heat.Features
{
    public class StationFeature : IStationFeature
    {
        public string All { get { return "transit.station"; } }
        public string Airport { get { return "transit.station.airport"; } }
        public string Bus { get { return "transit.station.bus"; } }
        public string Rail { get { return "transit.station.rail"; } }
    }
}
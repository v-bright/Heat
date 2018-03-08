namespace Heat.Features
{
    public class PoiFeature : IPoiFeature
    {
        public string All { get { return "poi"; } }
        public string Attraction { get { return "poi.attraction"; } }
        public string Business { get { return "poi.business"; } }
        public string Government { get { return "poi.government"; } }
        public string Medical { get { return "poi.medical"; } }
        public string Park { get { return "poi.park"; } }
        public string PlaceOfWorship { get { return "poi.place_of_worship"; } }
        public string School { get { return "poi.school"; } }
        public string SportsComplex { get { return "poi.sports_complex"; } }
    }
}
namespace Heat.Features
{
    public interface IAdministrativeFeature
    {
        string All { get; }
        string Country { get; }
        string LandParcel { get; }
        string Locality { get; }
        string Neighborhood { get; }
        string Province { get; }
    }
}
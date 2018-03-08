namespace Heat.Features
{
    public interface ILandscapeFeature
    {
        string All { get; }
        string ManMade { get; }
        INaturalFeature Natural { get; }
    }
}
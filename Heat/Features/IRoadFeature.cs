namespace Heat.Features
{
    public interface IRoadFeature
    {
        string All { get; }
        string Arterial { get; }
        string Local { get; }
        IHighwayFeature Highway { get; }
    }
}
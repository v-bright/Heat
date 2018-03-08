namespace Heat.Elements
{
    public interface IGeometryElement
    {
        string All { get; }
        string Fill { get; }
        string Stroke { get; }
        ILabelsElement Labels { get; }
    }
}
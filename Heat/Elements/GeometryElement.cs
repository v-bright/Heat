namespace Heat.Elements
{
    public class GeometryElement : IGeometryElement
    {
        public string All { get { return "geometry"; } }
        public string Fill { get { return "geometry.fill"; } }
        public string Stroke { get { return "geometry.stroke"; } }
        public ILabelsElement Labels { get { return new LabelsElement(); } }
    }
}
namespace Heat.Elements
{
    public class LabelsElement : ILabelsElement
    {
        public string All { get { return "labels"; } }
        public string Icon { get { return "icon"; } }
        public ITextElement Text { get { return new TextElement(); } }
    }
}
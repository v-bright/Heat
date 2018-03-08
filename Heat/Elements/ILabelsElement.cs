namespace Heat.Elements
{
    public interface ILabelsElement
    {
        string All { get; }
        string Icon { get; }
        ITextElement Text { get; }
    }
}
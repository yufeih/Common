namespace System.Runtime.Serialization
{
    public interface ITextConverter { }
    public interface ITextConverter<T> : ITextConverter
    {
        string ToText(T value);
        T FromText(string text);
    }
}

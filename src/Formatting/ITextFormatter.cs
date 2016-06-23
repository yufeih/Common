namespace Nine.Formatting
{
    using System;
    using System.IO;

    public interface ITextFormatter
    {
        void WriteTo(object value, TextWriter writer);

        object ReadFrom(Type type, TextReader reader);
    }
}

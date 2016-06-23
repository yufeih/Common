namespace Nine.Formatting
{
    using System;
    using System.IO;

    public interface IFormatter
    {
        void WriteTo(object value, Stream stream);

        object ReadFrom(Type type, Stream stream);
    }
}

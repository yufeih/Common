namespace Nine.Formatting
{
    using System;
    using System.IO;
    using System.Text;

    public static class FormatterExtensions
    {
        private static readonly Encoding s_encoding = new UTF8Encoding(false, true);

        public static T ReadFrom<T>(this IFormatter formatter, Stream stream)
        {
            return (T)formatter.ReadFrom(typeof(T), stream);
        }
        
        public static T FromBytes<T>(this IFormatter formatter, byte[] bytes)
        {
            var ms = new MemoryStream(bytes, writable: false);
            return (T)formatter.ReadFrom(typeof(T), ms);
        }
        
        public static T FromBytes<T>(this IFormatter formatter, byte[] bytes, int index, int count)
        {
            var ms = new MemoryStream(bytes, index, count, writable: false);
            return (T)formatter.ReadFrom(typeof(T), ms);
        }

        public static byte[] ToBytes(this IFormatter formatter, object value)
        {
            var ms = MemoryStreamCache.Acquire();
            formatter.WriteTo(value, ms);
            return MemoryStreamCache.GetBytesAndRelease(ms);
        }
        
        public static T ReadFrom<T>(this ITextFormatter formatter, Stream stream)
        {
            using (var reader = new StreamReader(stream, s_encoding, true, 1024, leaveOpen: true))
            {
                return (T)formatter.ReadFrom(typeof(T), new StreamReader(stream));
            }
        }

        public static object ReadFrom(this ITextFormatter formatter, Type type, Stream stream)
        {
            using (var reader = new StreamReader(stream, s_encoding, true, 1024, leaveOpen: true))
            {
                return formatter.ReadFrom(type, new StreamReader(stream));
            }
        }

        public static void WriteTo(this ITextFormatter formatter, object value, Stream stream)
        {
            using (var writer = new StreamWriter(stream, s_encoding, 1024, leaveOpen: true))
            {
                formatter.WriteTo(value, writer);
            }
        }

        public static object FromText(this ITextFormatter formatter, Type type, string text)
        {
            return formatter.ReadFrom(type, new StringReader(text));
        }

        public static T FromText<T>(this ITextFormatter formatter, string text)
        {
            return (T)formatter.ReadFrom(typeof(T), new StringReader(text));
        }

        public static string ToText(this ITextFormatter formatter, object value)
        {
            var sb = StringBuilderCache.Acquire(256);
            formatter.WriteTo(value, new StringWriter(sb));
            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}

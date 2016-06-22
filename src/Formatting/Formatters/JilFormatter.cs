namespace System.Runtime.Serialization
{
    using System;
    using System.IO;
    using System.Text;
    using Jil;

    public class JilFormatter : IFormatter, ITextFormatter
    {
        private readonly Encoding _encoding = new UTF8Encoding(false, true);
        private readonly Options _options;

        public JilFormatter()
        {
            _options = new Options(
                prettyPrint: false,
                excludeNulls: true,
                jsonp: false,
                dateFormat: Jil.DateTimeFormat.ISO8601,
                includeInherited: true,
                unspecifiedDateTimeKindBehavior: UnspecifiedDateTimeKindBehavior.IsUTC,
                serializationNameFormat: SerializationNameFormat.CamelCase);
        }

        public JilFormatter(Options options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _options = options;
        }

        public void WriteTo(object value, Stream stream)
        {
            using (var writer = new StreamWriter(stream, _encoding, 1024, leaveOpen: true))
            {
                JSON.Serialize(value, writer, _options);
            }
        }

        public object ReadFrom(Type type, Stream stream)
        {
            using (var reader = new StreamReader(stream, _encoding, true, 1024, leaveOpen: true))
            {
                return JSON.Deserialize(reader, type, _options);
            }
        }

        public void WriteTo(object value, TextWriter writer)
        {
            JSON.Serialize(value, writer, _options);
        }

        public object ReadFrom(Type type, TextReader reader)
        {
            return JSON.Deserialize(reader, type, _options);
        }
    }
}
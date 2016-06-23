namespace Nine.Formatting
{
    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public class JsonFormatter : IFormatter, ITextFormatter
    {
        public Formatting Formatting
        {
            get { return _json.Formatting; }
            set { _json.Formatting = value; }
        }

        private static readonly Encoding _encoding = new UTF8Encoding(false, true);
        private readonly JsonSerializer _json = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        public JsonFormatter(TextConverter textConverter = null, params JsonConverter[] converters)
        {
            if (converters != null)
            {
                foreach (var converter in converters)
                {
                    _json.Converters.Add(converter);
                }
            }

            if (textConverter != null)
            {
                _json.Converters.Add(new TextConverterJsonConverter(textConverter));
            }

            _json.Converters.Add(new NoThrowStringEnumConverter());
        }

        public void WriteTo(object value, Stream stream)
        {
            using (var writer = new StreamWriter(stream, _encoding, 1024, leaveOpen: true))
            {
                _json.Serialize(writer, value);
            }
        }

        public object ReadFrom(Type type, Stream stream)
        {
            using (var reader = new StreamReader(stream, _encoding, true, 1024, leaveOpen: true))
            {
                return _json.Deserialize(reader, type);
            }
        }

        public void WriteTo(object value, TextWriter writer)
        {
            _json.Serialize(writer, value);
        }

        public object ReadFrom(Type type, TextReader reader)
        {
            return _json.Deserialize(reader, type);
        }

        class NoThrowStringEnumConverter : StringEnumConverter
        {
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                try
                {
                    return base.ReadJson(reader, objectType, existingValue, serializer);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        class TextConverterJsonConverter : JsonConverter
        {
            private readonly TextConverter _converter;

            public TextConverterJsonConverter(TextConverter converter) { _converter = converter; }

            public override bool CanConvert(Type objectType)
                => _converter.CanConvert(objectType);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                => _converter.FromText(objectType, reader.Value.ToString());

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                => _converter.ToText(value);
        }
    }
}

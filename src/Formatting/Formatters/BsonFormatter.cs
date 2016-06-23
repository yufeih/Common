namespace Nine.Formatting
{
    using System;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;
    using Newtonsoft.Json.Serialization;

    public class BsonFormatter : IFormatter
    {
        private static readonly Encoding _encoding = new UTF8Encoding(false, true);
        private readonly JsonSerializer _json = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        public BsonFormatter(params JsonConverter[] defaultConverters)
        {
            if (defaultConverters != null)
            {
                foreach (var converter in defaultConverters)
                {
                    _json.Converters.Add(converter);
                }
            }
            _json.Converters.Add(new TimeSpanConverter());
            _json.Converters.Add(new DateTimeConverter());
        }

        public object ReadFrom(Type type, Stream stream)
        {
            using (var binary = new BinaryReader(stream, _encoding, leaveOpen: true))
            using (var reader = new BsonReader(binary))
            {
                return _json.Deserialize(reader, type);
            }
        }

        public void WriteTo(object value, Stream stream)
        {
            using (var binary = new BinaryWriter(stream, _encoding, leaveOpen: true))
            using (var writer = new BsonWriter(binary))
            {
                writer.Formatting = _json.Formatting;
                _json.Serialize(writer, value);
            }
        }
        
        class TimeSpanConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(TimeSpan);

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                => writer.WriteValue(((TimeSpan)value).Ticks);

            public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer serializer)
                => TimeSpan.FromTicks((long)reader.Value);
        }

        class DateTimeConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(DateTime);

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                => writer.WriteValue(((DateTime)value).Ticks);

            public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer serializer)
                => new DateTime((long)reader.Value, DateTimeKind.Utc);
        }
    }
}

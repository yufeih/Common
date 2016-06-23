namespace Nine.Formatting
{
    using System;
    using System.IO;
    using ProtoBuf.Meta;

    public class ProtoFormatter : IFormatter
    {
        private readonly RuntimeTypeModel _model;

        public ProtoFormatter()
        {
            _model = TypeModel.Create();
            _model.IncludeDateTimeKind = true;
        }

        public object ReadFrom(Type type, Stream stream)
        {
            return _model.Deserialize(stream, null, type);
        }

        public void WriteTo(object value, Stream stream)
        {
            _model.Serialize(stream, value);
        }
    }
}

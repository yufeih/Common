namespace Nine.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class TextConverter
    {
        private readonly Dictionary<Type, IConverter> _converters;

        public TextConverter() { }
        public TextConverter(params ITextConverter[] converters)
        {
            _converters = converters
                .GroupByGenericTypeArgument(typeof(ITextConverter<>), false)
                .ToDictionary(p => p.Key, p => (IConverter)Activator.CreateInstance(typeof(Converter<>).MakeGenericType(p.Key), p.Value));
        }

        public bool CanConvert(Type type)
        {
            return GetConverter(type) != null;
        }

        private IConverter GetConverter(Type type)
        {
            if (_converters == null) return null;
            IConverter converter = null;
            while (type != null)
            {
                if (_converters.TryGetValue(type, out converter)) break;
                type = type.GetTypeInfo().BaseType;
            }
            return converter;
        }

        public virtual bool ToText(object value, out string result)
        {
            if (value == null)
            {
                result = null;
                return false;
            }

            var converter = GetConverter(value.GetType());
            if (converter != null)
            {
                result = converter.ToText(value);
                return true;
            }

            result = null;
            return false;
        }

        public string ToText(object value)
        {
            string result;
            return ToText(value, out result) ? result : value?.ToString();
        }

        public virtual bool FromText(Type type, string text, out object value)
        {
            var converter = GetConverter(type);
            if (converter != null)
            {
                value = converter.FromText(text);
                return true;
            }

            value = null;
            return false;
        }

        public object FromText(Type type, string text)
        {
            object result;
            return FromText(type, text, out result) ? result : null;
        }

        interface IConverter
        {
            string ToText(object value);
            object FromText(string text);
        }

        class Converter<T> : IConverter
        {
            private readonly ITextConverter<T> _converter;

            public Converter(ITextConverter<T> converter) { _converter = converter; }

            public object FromText(string text) => _converter.FromText(text);

            public string ToText(object value) => _converter.ToText((T)value);
        }
    }
}

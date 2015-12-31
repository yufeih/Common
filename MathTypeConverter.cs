namespace System
{
    using Globalization;
    using System.Linq;

    static class MathTypeConverter
    {
        public static T[] FromText<T>(string text, int count, Func<string, T> parse)
        {
            if (string.IsNullOrEmpty(text)) return null;

            var i = 0;
            var result = new T[count];
            foreach (var part in text.Split(',').Take(count).Select(p => p.Trim()))
            {
                if (string.IsNullOrEmpty(part))
                {
                    result[i++] = default(T);
                }
                else
                {
                    result[i++] = parse(part);
                }
            }
            return result;
        }

        public static string ToText(params float[] values)
        {
            for (var i = values.Length - 1; i >= 0; i--)
            {
                if (values[i] != 0)
                {
                    return string.Join(",", values.Take(i + 1).Select(_ => _.ToString(CultureInfo.InvariantCulture)));
                }
            }
            return "";
        }

        public static string ToText(params double[] values)
        {
            for (var i = values.Length - 1; i >= 0; i--)
            {
                if (values[i] != 0)
                {
                    return string.Join(",", values.Take(i + 1).Select(_ => _.ToString(CultureInfo.InvariantCulture)));
                }
            }
            return "";
        }

        public static string ToText<T>(params T[] values)
        {
            for (var i = values.Length - 1; i >= 0; i--)
            {
                // Ignore default values
                if (!Equals(values[i], default(T)))
                {
                    return string.Join(",", values.Take(i + 1));
                }
            }
            return "";
        }
    }
}

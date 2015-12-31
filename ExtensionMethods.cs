namespace System
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    [EditorBrowsable(EditorBrowsableState.Never)]
    static class ExtensionMethods
    {
        #region Collection
        public static void AddRange<T>(this ICollection<T> collection, params T[] values)
        {
            if (collection != null && values != null)
            {
                foreach (var value in values)
                {
                    collection.Add(value);
                }
            }
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            if (collection != null && values != null)
            {
                foreach (var value in values)
                {
                    collection.Add(value);
                }
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> collection, T value)
        {
            if (collection != null)
            {
                var result = 0;
                foreach (var item in collection)
                {
                    if (object.Equals(item, value)) return result;
                    result++;
                }
            }
            return -1;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> collection, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (collection != null)
            {
                TValue value;
                if (!collection.TryGetValue(key, out value))
                {
                    collection.Add(key, value = valueFactory(key));
                }
                return value;
            }
            return default(TValue);
        }

        public static void Replace<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            if (collection != null && values != null)
            {
                collection.Clear();
                foreach (var value in values)
                {
                    collection.Add(value);
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> values, int countPerBatch)
        {
            var result = new List<T>();

            foreach (var item in values)
            {
                result.Add(item);
                if (result.Count >= countPerBatch)
                {
                    yield return result;
                    result = new List<T>();
                }
            }

            if (result.Count > 0)
            {
                yield return result;
            }
        }

        private static readonly Random random = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        #endregion

        #region Misc
        public static T GetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            if (serviceProvider == null)
            {
                return null;
            }

            return serviceProvider.GetService(typeof(T)) as T;
        }
        #endregion

        #region IO
        public static async Task<byte[]> ReadBytesAsync(this Stream input, int bufferSize = 8 * 1024, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (input == null) throw new ArgumentNullException("input");

            // http://stackoverflow.com/questions/221925/creating-a-byte-array-from-a-stream
            byte[] buffer = new byte[bufferSize];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static async Task CopyToAsync(this Stream input, Stream output, int bufferSize, IProgress<Tuple<int, int>> progress, int totalBytes = 0)
        {
            var currentBytes = 0;
            var buffer = new byte[bufferSize];

            if (totalBytes <= 0 && input.CanSeek)
            {
                try
                {
                    totalBytes = (int)input.Length;
                }
                catch (NotSupportedException) { }
            }

            if (progress != null)
            {
                progress.Report(Tuple.Create(currentBytes, Math.Max(currentBytes, totalBytes)));
            }

            while (true)
            {
                var bytesRead = await input.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false);

                if (bytesRead <= 0)
                {
                    if (progress != null)
                    {
                        progress.Report(Tuple.Create(currentBytes, Math.Max(currentBytes, totalBytes)));
                    }
                    return;
                }

                currentBytes += bytesRead;

                await output.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);

                if (progress != null)
                {
                    progress.Report(Tuple.Create(currentBytes, Math.Max(currentBytes, totalBytes)));
                }
            }
        }

        public static async Task CopyToAsync(this Stream input, Stream output, int bufferSize, int maxSizeInBytes, Action onExceededMaxSize = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var totalBytes = 0;
            var buffer = new byte[bufferSize];

            while (true)
            {
                var bytesRead = await input.ReadAsync(buffer, 0, bufferSize, cancellationToken).ConfigureAwait(false);
                if (bytesRead <= 0)
                {
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                if ((totalBytes += bytesRead) > maxSizeInBytes)
                {
                    if (onExceededMaxSize != null)
                    {
                        onExceededMaxSize();
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("Input stream is bigger than the upper bound");
                    }
                }

                await output.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            }
        }
        #endregion

        #region Linq
        public static TimeSpan Average(this IEnumerable<TimeSpan> source)
        {
            return TimeSpan.FromTicks((long)Enumerable.Average(source, x => x.Ticks));
        }

        public static TimeSpan Average<T>(this IEnumerable<T> source, Func<T, TimeSpan> selector)
        {
            return TimeSpan.FromTicks((long)Enumerable.Average(source, x => selector(x).Ticks));
        }
        #endregion

        #region Text
        private static readonly char[] _trimChars = new[] { ' ', '\r', '\n', '\t' };

        public static string TrimAll(this string value)
        {
            if (value == null) return "";
            return value.Trim().Trim(_trimChars).Replace("\r", "").Replace("\n", " ").Replace("\t", " ");
        }

        private static DateTime unixTimeStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime ToDateTime(this long unixTimestamp)
        {
            // http://stackoverflow.com/questions/4964634/how-to-convert-long-type-datetime-to-datetime-with-correct-time-zone            
            return unixTimeStart.AddMilliseconds(unixTimestamp);
        }

        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null) return "";

            // http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(87 + b + (((b - 10) >> 31) & -39));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(87 + b + (((b - 10) >> 31) & -39));
            }
            return new string(c);
        }

        #endregion

        #region Reflection

        public static IEnumerable<Type> TryGetExportedTypes(this Assembly assembly)
        {
            try
            {
                if (assembly.IsDynamic) return Enumerable.Empty<Type>();

                return assembly.ExportedTypes;
            }
            catch (ReflectionTypeLoadException e)
            {
                Debug.WriteLine("Error loading assembly: " + assembly.FullName);
                Debug.WriteLine(e.LoaderExceptions[0]);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error loading assembly: " + assembly.FullName);
                Debug.WriteLine(e);
            }
            return Enumerable.Empty<Type>();
        }

        public static object TryParseEnum(Type enumType, string text)
        {
            try
            {
                return Enum.Parse(enumType, text, true);
            }
            catch (ArgumentException)
            {
                return 0;
            }
        }

        public static Dictionary<Type, T> GroupByGenericTypeDefinition<T>(this T[] values, Type typeDefinition, bool throwOnDuplicate = true, int genericTypeIndex = 0)
        {
            var result = new Dictionary<Type, T>();
            if (values == null) return result;

            foreach (var value in values)
            {
                if (value == null) continue;

                var types =
                    from i in value.GetType().GetTypeInfo().ImplementedInterfaces
                    where i.GetTypeInfo().IsGenericType
                    let d = i.GetGenericTypeDefinition()
                    where d == typeDefinition
                    select i.GenericTypeArguments[genericTypeIndex];

                foreach (var type in types)
                {
                    T existing;
                    if (result.TryGetValue(type, out existing))
                    {
                        var error = type.FullName + " is implemented by both " + existing.GetType().FullName + " and " + value.GetType().FullName;
                        if (throwOnDuplicate) throw new InvalidOperationException(error);
                        Debug.WriteLine(error);
                    }
                    result[type] = value;
                }
            }
            return result;
        }

        #endregion
    }
}

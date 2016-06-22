namespace System.Collections.Generic
{
    static class Empty
    {
        public static T[] Array<T>() => Backing<T>.Array;
        public static List<T> List<T>() => Backing<T>.List;
        public static Dictionary<TKey, TValue> Dictionary<TKey, TValue>() => Backing<TKey, TValue>.Dictionary;

        class Backing<T>
        {
            public static readonly T[] Array = new T[0];
            public static readonly List<T> List = new List<T>(0);
        }

        class Backing<T1, T2>
        {
            public static readonly Dictionary<T1, T2> Dictionary = new Dictionary<T1, T2>(0);
        }
    }

    static class CollectionExtensions
    {
        private static readonly Random s_random = new Random();

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

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = s_random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
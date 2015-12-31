namespace System.Collections.Generic
{
    class DictionaryInitializer<TKey, TValue>
    {
        private readonly Func<TKey, TValue> valueFactory;
        private readonly Dictionary<TKey, TValue> cache;

        public DictionaryInitializer(Func<TKey, TValue> valueFactory, IEqualityComparer<TKey> comparer = null)
        {
            if (valueFactory == null) throw new ArgumentNullException("valueFactory");

            this.valueFactory = valueFactory;
            this.cache = new Dictionary<TKey, TValue>(comparer ?? EqualityComparer<TKey>.Default);
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (cache)
                {
                    TValue result;

                    if (cache.TryGetValue(key, out result)) return result;

                    return cache[key] = valueFactory(key);
                }
            }
        }
    }
}

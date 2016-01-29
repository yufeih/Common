namespace System
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    [DebuggerStepThrough]
    static class TestExtensionMethods
    {
        public async static Task PropertyChangedTo<T>(this T obj, Func<T, bool> predicate, int timeout = 10000)
        {
            if (predicate(obj)) return;

            var n = obj as INotifyPropertyChanged;
            if (n == null) throw new TimeoutException();

            var tcs = new TaskCompletionSource<int>();
            var handler = (PropertyChangedEventHandler)null;
            handler = new PropertyChangedEventHandler((sender, e) =>
            {
                if (predicate(obj))
                {
                    n.PropertyChanged -= handler;
                    tcs.TrySetResult(0);
                }
            });
            n.PropertyChanged += handler;

            var delay = Task.Delay(timeout);
            if (delay == await Task.WhenAny(delay, tcs.Task))
            {
                n.PropertyChanged -= handler;
                throw new TimeoutException($"{ JsonConvert.ToString(obj) }");
            }
        }

        public static Task CollectionChangedTo<T>(this IEnumerable<T> collection, params Func<T, bool>[] elementPredicate)
        {
            var predicate = new Func<T[], bool>(array =>
            {
                if (array.Length != elementPredicate.Length) return false;
                for (int i = 0; i < array.Length; i++) if (!elementPredicate[i](array[i])) return false;
                return true;
            });
            return CollectionChangedTo(collection, predicate);
        }

        public async static Task CollectionChangedTo<T>(this IEnumerable<T> collection, Func<T[], bool> predicate, int timeout = 10000)
        {
            if (predicate(collection.ToArray())) return;

            var collectionChanged = collection as INotifyCollectionChanged;
            if (collectionChanged == null) throw new TimeoutException();

            var tcs = new TaskCompletionSource<int>();
            var handler = (NotifyCollectionChangedEventHandler)null;
            handler = new NotifyCollectionChangedEventHandler((sender, e) =>
            {
                if (predicate(collection.ToArray()))
                {
                    collectionChanged.CollectionChanged -= handler;
                    tcs.TrySetResult(0);
                }
            });
            collectionChanged.CollectionChanged += handler;

            var delay = Task.Delay(timeout);
            if (delay == await Task.WhenAny(delay, tcs.Task))
            {
                collectionChanged.CollectionChanged -= handler;
                throw new TimeoutException($"[{ collection.Count() }] : { JsonConvert.ToString(collection) }");
            }
        }
    }
}
namespace System.Threading.Tasks
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    static class Tasks
    {
        public static readonly Task Completed = Task.FromResult(true);

        public static readonly Task<bool> True = Task.FromResult(true);
        public static readonly Task<bool> False = Task.FromResult(false);
        public static readonly Task<string> NullString = Task.FromResult<string>(null);
        public static readonly Task<string> EmptyString = Task.FromResult("");
        public static readonly Task<Stream> NullStream = Task.FromResult<Stream>(null);

        public static Task<T> Null<T>() where T : class => Nulls<T>.Value;
        public static Task<T> Default<T>() => Defaults<T>.Value;
        public static Task<IEnumerable<T>> Empty<T>() => Emptys<T>.Value;
        public static Task<T[]> EmptyArray<T>() => EmptyArrays<T>.Value;

        class Defaults<T> { public static readonly Task<T> Value = Task.FromResult(default(T)); }
        class Nulls<T> where T : class { public static readonly Task<T> Value = Task.FromResult<T>(null); }
        class Emptys<T> { public static readonly Task<IEnumerable<T>> Value = Task.FromResult(Enumerable.Empty<T>()); }
        class EmptyArrays<T> { public static readonly Task<T[]> Value = Task.FromResult(new T[0]); }
    }

    static class TaskExtensions
    {
        public static void Ignore(this Task task) { }
    }
}
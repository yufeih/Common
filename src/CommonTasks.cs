namespace System.Threading.Tasks
{
    using System.IO;

    static class CommonTasks
    {
        public static readonly Task<bool> True = Task.FromResult(true);
        public static readonly Task<bool> False = Task.FromResult(false);
        public static readonly Task<string> NullString = Task.FromResult<string>(null);
        public static readonly Task<string> EmptyString = Task.FromResult("");
        public static readonly Task<Stream> NullStream = Task.FromResult<Stream>(null);
    }
}
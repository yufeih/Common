namespace System.Threading.Tasks
{
    using System.Collections.Generic;

    /// <summary>
    /// Enables tasks to be executed sequentially.
    /// </summary>
    /// <remarks>
    /// This class is currently not thread safe and should only be used in the client side where the synchronization context always
    /// post callbacks to the same thread.
    /// </remarks>
    class TaskQueue
    {
        private readonly Dictionary<string, Entry> taskQueues = new Dictionary<string, Entry>();

        public Task<T> QueueAsync<T>(string key, Func<Task<T>> taskFactory)
        {
            Entry entry;
            if (!taskQueues.TryGetValue(key, out entry)) taskQueues.Add(key, entry = new Entry());
            return entry.QueueAsync(taskFactory);
        }

        class Entry
        {
            private Task lastTask;

            /// <summary>
            /// Gets whether this queue is empty.
            /// </summary>
            public bool IsEmpty
            {
                get
                {
                    // Cache as local var to avoid race condition.
                    var task = lastTask;
                    return task == null || task.IsCompleted || task.IsFaulted || task.IsCanceled;
                }
            }

            /// <summary>
            /// Adds the specified task to the queue to be executed after all the queued tasks has finished.
            /// </summary>
            public async Task<T> QueueAsync<T>(Func<Task<T>> taskFactory)
            {
                if (taskFactory == null) throw new ArgumentNullException("taskFactory");

                var tcs = new TaskCompletionSource<T>();
                var previousTask = Interlocked.Exchange(ref lastTask, tcs.Task);

                if (previousTask != null)
                {
                    try
                    {
                        await previousTask;
                    }
                    catch
                    {
                        // Ignore previous errors
                    }
                }

                try
                {
                    var result = await taskFactory();
                    tcs.SetResult(result);
                    return result;
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                    throw;
                }
            }
        }
    }
}
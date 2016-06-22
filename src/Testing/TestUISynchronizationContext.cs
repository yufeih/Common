namespace System.Threading
{
    using System.Collections.Concurrent;
    using System.Diagnostics;

    [DebuggerStepThrough]
    class TestUISynchronizationContext : SynchronizationContext, IDisposable
    {
        struct WorkItem
        {
            public SendOrPostCallback Callback;
            public object State;
            public AutoResetEvent Handle;
        }

        public static SynchronizationContext BindToCurrent()
        {
            var result = new TestUISynchronizationContext();
            SetSynchronizationContext(result);
            return result;
        }
        
        private bool _disposed;
        private readonly SynchronizationContext _previous = Current;
        private readonly Thread _uiThread;
        private readonly BlockingCollection<WorkItem> _workItems = new BlockingCollection<WorkItem>();
        
        public TestUISynchronizationContext()
        {
            _uiThread = new Thread(Loop) { Name = "UIThread" };
            _uiThread.Start();
        }

        private void Loop(object obj)
        {
            SetSynchronizationContext(this);

            while (!_disposed)
            {
                var currentItem = _workItems.Take();

                try
                {
                    currentItem.Callback(currentItem.State);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
                finally
                {
                    currentItem.Handle?.Set();
                }
            }
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _workItems.Add(new WorkItem { Callback = d, State = state });
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (Thread.CurrentThread == _uiThread)
            {
                d(state);
            }
            else
            {
                var workItem = new WorkItem { Callback = d, State = state, Handle = new AutoResetEvent(false) };
                _workItems.Add(workItem);
                workItem.Handle.WaitOne();
            }
        }

        public void Dispose()
        {
            _disposed = true;
            SetSynchronizationContext(_previous);
        }
    }
}
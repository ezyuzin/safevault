using System;
using System.Collections.Generic;
using System.Threading;

namespace SafeVault.Threading
{
    public class QueueProcessor<T>
    {
        public string Name { get; set; }

        private readonly object _queueLock = new object();
        private readonly LinkedList<T> _queue = new LinkedList<T>();
        
        private readonly ManualResetEvent _workerCompletedEvent = new ManualResetEvent(true);
        private readonly ManualResetEvent _queueItemAddedEvent = new ManualResetEvent(false);
        private readonly object _workerThreadLock = new object();
        private Thread _workerThread;


        private bool _disposing = false;
        private bool _stopWorker = false;
        private readonly Action<T> _action;

        public QueueProcessor(string name, Action<T> processor)
        {
            _action = processor;
            Name = name;
        }

        public void Dispose()
        {
            if (_disposing)
                return;

            _disposing = true;

            if (GetQueueLength() != 0)
                _workerCompletedEvent.WaitOne(1000);

            _stopWorker = true;
            _queueItemAddedEvent.Set();
            if (!_workerCompletedEvent.WaitOne(1000))
            {
                _workerThread?.Abort();
            }

            #if NETFX
            _workerCompletedEvent.Close();
            _queueItemAddedEvent.Close();
            
            #endif

            #if NETSTANDARD2_0
            _workerCompletedEvent.Dispose();
            _queueItemAddedEvent.Dispose();
            #endif
        }

        public int GetQueueLength()
        {
            lock (_queue)
            {
                return _queue.Count;
            }
        }

        public void WorkerAction()
        {
            //Console.WriteLine($"{DateTime.Now} Worker: Started");
            try
            {
                while (true)
                {
                    //Console.WriteLine($"{DateTime.Now} Worker: Waiting Item");
                    if (!_queueItemAddedEvent.WaitOne(5000) || _stopWorker)
                        break;

                    T rec;
                    lock (_queue)
                    {
                        if (_queue.Count != 0)
                        {
                            rec = _queue.First.Value;
                            _queue.RemoveFirst();
                            //Console.WriteLine($"{DateTime.Now} Worker: Item received");
                        }
                        else
                        {
                            //Console.WriteLine($"{DateTime.Now} Worker: Queue empty");
                            _queueItemAddedEvent.Reset();
                            continue;
                        }
                    }

                    try
                    {
                        _action.Invoke(rec);
                    }
                    catch (Exception)
                    {
                        // just eat all unprocessed exceptiong within action :-(
                    }
                }
            }
            finally
            {
                _workerThread = null;
                _workerCompletedEvent.Set();
            }
            //Console.WriteLine($"{DateTime.Now} Worker: Stopped");
        }

        private void RunWorker()
        {
            if (_disposing || _stopWorker || _workerThread != null)
                return;

            Thread thread = new Thread(WorkerAction);
            lock (_workerThreadLock)
            {
                if (_disposing || _stopWorker || _workerThread != null)
                    return;

                _workerThread = thread;
            }

            _workerCompletedEvent.Reset();
            _workerThread.Start();
        }

        public void Append(T item)
        {
            if (_disposing)
                return;

            bool runWorker = false;
            lock (_queueLock)
            {
                if (!_disposing)
                {
                    _queue.AddLast(item);
                    _queueItemAddedEvent.Set();
                    runWorker = true;
                }
            }

            if (runWorker)
                RunWorker();
        }
    }
}
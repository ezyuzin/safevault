using System;
using System.Threading;

namespace SafeVault.Net.SendMail
{
    public class SendMailWorker : IDisposable
    {
        private Exception _actionException;
        private AutoResetEvent _sendCompleted;
        private Thread _sendThread;

        public void Dispose()
        {
            #if NETSTANDARD2_0
            _sendCompleted?.Dispose();
            #endif
            #if NETFX
            _sendCompleted?.Close();
            _sendThread?.Abort();
            _sendThread = null;
            #endif

            _sendCompleted = null;
        }

        public void Run(Action action)
        {
            if (_sendThread != null)
                throw new ApplicationException("Reuse SmtpSendWorker is not allowed");

            _sendCompleted = new AutoResetEvent(false);
            _sendThread = new Thread(() => Worker(action));
            _sendThread.Start();
        }

        public bool WaitOne(TimeSpan ts)
        {
            if (_sendCompleted?.WaitOne(ts) == false)
                return false;

            if (_actionException != null)
            {
                throw _actionException;
            }
            return true;
        }

        private void Worker(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                _actionException = e;
            }
            _sendThread = null;
            _sendCompleted.Set();
        }


    }
}
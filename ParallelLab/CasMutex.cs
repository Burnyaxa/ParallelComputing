using System.Collections.Generic;
using System.Threading;

namespace ParallelLab
{
    public class CasMutex
    {
        private volatile int _isLocked = 0;
        private Thread _lockedThread;
        private volatile bool _isNotified;
        private volatile bool _isNotifiedAll;
        private readonly SynchronizedCollection<Thread> _threads = new SynchronizedCollection<Thread>();
        public void Lock()
        {
            var currentValue = _isLocked;
            while (Interlocked.CompareExchange(ref _isLocked, 0, currentValue) == 0)
            {
                Thread.Yield();
                currentValue = _isLocked;
            }
            _lockedThread = Thread.CurrentThread;
        }

        public void Unlock()
        {
            _lockedThread = null;
            _isLocked = 0;
        }

        public void Wait()
        {
            if (_lockedThread != Thread.CurrentThread)
            {
                throw new ThreadStateException();
            }
            Unlock();
            _threads.Add(Thread.CurrentThread);
            while (!_isNotified || !_isNotifiedAll)
            {
                Thread.Yield();
            }
            _threads.Remove(Thread.CurrentThread);
            Lock();
            _isNotified = false;
        }

        public void Notify()
        {
            if (_lockedThread != Thread.CurrentThread)
            {
                throw new ThreadStateException();
            }

            _isNotified = true;
        }

        public void NotifyAll()
        {
            if (_lockedThread != Thread.CurrentThread)
            {
                throw new ThreadStateException();
            }

            _isNotifiedAll = true;
            
            while (_threads.Count != 0)
            {
                
            }

            _isNotifiedAll = false;

        }
    }
}
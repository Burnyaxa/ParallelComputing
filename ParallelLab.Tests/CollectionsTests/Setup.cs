using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelLab.Tests.CollectionsTests
{
    public class Setup
    {
        private readonly object _obj = new object();
        private readonly Random _random = new Random();
        
        public List<Thread> Threads = new List<Thread>();

        public void RunActions(Action<object> action, int operationCount)
        {
            var threads = new List<Thread>();
            for (var i = 0; i < operationCount; i++)
            {
                threads.Add(new Thread(new ParameterizedThreadStart(action)));
            }

            Parallel.ForEach(threads, t =>
            {
                int value;
                lock (_obj)
                {
                    value = _random.Next(0, 10000);
                }
                t.Start(value);
            });

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
    }
}
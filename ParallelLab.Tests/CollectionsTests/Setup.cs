using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelLab.Tests.CollectionsTests
{
    public class Setup
    {
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
                t.Start(_random.Next(0, 10000));
            });

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
    }
}
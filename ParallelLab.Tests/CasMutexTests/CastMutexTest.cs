using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ParallelLab.Tests.CasMutexTests
{
    public class CastMutexTest
    {
        private const int MaxCapacity = 3;
        private CasMutex _mutex;
        private Queue<int> _nums;
        private Random _random;
        
        [SetUp]
        public void Setup()
        {
            _mutex = new CasMutex();
            _nums = new Queue<int>();
            _random = new Random();
        }
        [Test]
        public void CasMutexTestPerformance()
        {
            var threads = new List<Thread>()
            {
                new Thread(Enqueue),
                new Thread(Enqueue),
                new Thread(Dequeue),
                new Thread(Enqueue),
                new Thread(Dequeue),
                new Thread(Enqueue),
                new Thread(Dequeue),
                new Thread(Dequeue),
                new Thread(Dequeue),
                new Thread(Enqueue)
            };

            Parallel.ForEach(threads, t =>
            {
                t.Start(_random.Next(0, 10));
            });

            foreach (var thread in threads)
            {
                thread.Join();
            }
            
            Assert.That(_nums.Count == 0);
        }
        
        
        private void Enqueue(object? obj)
        {
            _mutex.Lock();
            while (_nums.Count >= MaxCapacity)
            {
                _mutex.Wait();
            }
            _nums.Enqueue((int) obj);
            _mutex.NotifyAll();
            Thread.Sleep(500);
            _mutex.Unlock();
        }

        private void Dequeue(object? obj)
        {
            _mutex.Lock();

            while (_nums.Count == 0)
            {
                _mutex.Wait();
            }
            _nums.Dequeue();
            _mutex.NotifyAll();
            Thread.Sleep(1000);
            _mutex.Unlock();
        }
    }
}
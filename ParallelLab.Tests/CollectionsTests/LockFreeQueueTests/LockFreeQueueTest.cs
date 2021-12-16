using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ParallelLab.Tests.CollectionsTests.LockFreeQueueTests
{
    public class LockFreeQueueTest
    {
        private LockFreeQueue<int> _queue;
        private SynchronizedCollection<int> _addedValues;
        private SynchronizedCollection<int> _removedVales;
        private Setup _setup;
        
        [SetUp]
        public void SetUp()
        {
            _queue = new LockFreeQueue<int>();
            _addedValues = new SynchronizedCollection<int>();
            _removedVales = new SynchronizedCollection<int>();
            _setup = new Setup();
        }

        [Test]
        public void LockFreeQueueTestPerformance()
        {
            _setup.RunActions(AddToCollection, 10);
            _setup.RunActions(RemoveFromCollection, 10);
            var sortedAddedValues = _addedValues.OrderBy(x => x);
            var sortedRemovedValues = _removedVales.OrderBy(x => x);
            
            CollectionAssert.AreEqual(sortedAddedValues, sortedRemovedValues);
        }

        private void AddToCollection(object? obj)
        {
            _queue.Enqueue((int) obj);
            _addedValues.Add((int) obj);
        }

        private void RemoveFromCollection(object? obj)
        {
            if (_queue.Dequeue(out var result))
            {
                _removedVales.Add(result);
            }
        }
    }
}
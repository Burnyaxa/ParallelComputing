using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ParallelLab.Util;

namespace ParallelLab.Tests.CollectionsTests.LinkedListTests
{
    public class LinkedListTest
    {
        private LockFreeLinkedList<BorderlineInteger, int> _list;
        private SynchronizedCollection<int> _addedValues;
        private SynchronizedCollection<int> _removedVales;
        private ConcurrentStack<int> _pendingItems;
        private Setup _setup;
        
        [SetUp]
        public void SetUp()
        {
            _list = new LockFreeLinkedList<BorderlineInteger, int>();
            _addedValues = new SynchronizedCollection<int>();
            _removedVales = new SynchronizedCollection<int>();
            _pendingItems = new ConcurrentStack<int>();
            _setup = new Setup();
        }

        [Test]
        public void LinkedListPerformance()
        {
            _setup.RunActions(AddToCollection, 1);
            _setup.RunActions(RemoveFromCollection, 1);
            var sortedAddedValues = _addedValues.OrderBy(x => x);
            var sortedRemovedValues = _removedVales.OrderBy(x => x);
            
            CollectionAssert.AreEqual(sortedAddedValues, sortedRemovedValues);
        }

        private void AddToCollection(object? obj)
        {
            _list.Add((int) obj);
            _pendingItems.Push((int) obj);
            _addedValues.Add((int) obj);
        }

        private void RemoveFromCollection(object? obj)
        {
            if (_pendingItems.TryPop(out var value))
            {
                if (_list.Remove(value))
                {
                    _removedVales.Add(value);
                }
            }
        }
    }
}
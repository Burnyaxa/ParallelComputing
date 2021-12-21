using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ParallelLab.Tests.CollectionsTests.SkipListTests
{
    public class SkipListTest
    {
        private SkipList<int> _list;
        private SynchronizedCollection<int> _addedValues;
        private SynchronizedCollection<int> _removedVales;
        private ConcurrentStack<Node<int>> _nodes;
        private Setup _setup;
        private Random _random;
        private readonly object randomLock = new();
        
        [SetUp]
        public void SetUp()
        {
            _list = new SkipList<int>();
            _addedValues = new SynchronizedCollection<int>();
            _removedVales = new SynchronizedCollection<int>();
            _nodes = new ConcurrentStack<Node<int>>();
            _setup = new Setup();
            _random = new Random();
        }

        [Test]
        public void LockFreeSkipListTestPerformance()
        {
            _setup.RunActions(AddToCollection, 10);
            _setup.RunActions(RemoveFromCollection, 10);
            var sortedAddedValues = _addedValues.OrderBy(x => x);
            var sortedRemovedValues = _removedVales.OrderBy(x => x);
            
            CollectionAssert.AreEqual(sortedAddedValues, sortedRemovedValues);
        }
       

        private void AddToCollection(object? obj)
        {
            int key;
            lock (randomLock)
            {
                key = _random.Next(0, 100000);
            }
            var node = new Node<int>((int) obj, key);
            var added = _list.Add(node);
            if (added)
            {
                _nodes.Push(node);
                _addedValues.Add(node.Value);
            }
        }

        private void RemoveFromCollection(object? obj)
        {
            if (!_nodes.TryPop(out var result)) return;
            if (_list.Remove(result))
            {
                _removedVales.Add(result.Value);
            }
        }
    }
}
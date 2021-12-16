using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ParallelLab.Util;

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
        [Repeat(300)]
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
            var key = _random.Next(0, 100000);
            var node = new Node<int>((int) obj, key);
            _list.Add(node);
            _nodes.Push(node);
            _addedValues.Add(node.Value);
        }

        private void RemoveFromCollection(object? obj)
        {
            if (!_nodes.TryPop(out var result)) return;
            if (_list.Remove(result))
            {
                _removedVales.Add(result.Value);
            }
        }
        
        // private static void PrintSkipListForm<T>(SkipList<T> target) where T : IComparable<T>{
        //     for (int i = SkipListSettings.MaxLevel; i >= 0; i--){
        //         Console.Write("{0:00}|", i);
        //         bool marked = false;
        //         var node = target.Head.Next[i].Get(ref marked);
        //         while (node != target.Tail){
        //             Console.Write(node.TopLevel >= i ? $"{node.NodeValue.Value} " : " ");
        //             node = node.Next[i].Get(ref marked);
        //         }
        //
        //         Console.WriteLine();
        //     }
        //
        //     Console.WriteLine("----------------------------");
        // }
    }
}
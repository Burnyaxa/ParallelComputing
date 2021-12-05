using System;
using ParallelLab.Util;

namespace ParallelLab
{
    public class SkipList<T>
    {
        private readonly Node<T> _head = new(int.MinValue);

        private readonly Node<T> _tail = new(int.MaxValue);

        public SkipList()
        {
            for (var i = 0; i < _head.Next.Length; ++i)
            {
                _head.Next[i] = new MarkedReference<Node<T>>(_tail, false);
            }
        }
        
        public bool Add(Node<T> node)
        {
            var preds = new Node<T>[SkipListSettings.MaxLevel + 1];
            var succs = new Node<T>[SkipListSettings.MaxLevel + 1];
            
            while (true)
            {
                {
                    var topLevel = node.TopLevel;

                    for (var level = SkipListSettings.MinLevel; level <= topLevel; ++level)
                    {
                        var tempSucc = succs[level];
                        node.Next[level] = new MarkedReference<Node<T>>(tempSucc, false);
                    }

                    var pred = preds[SkipListSettings.MinLevel];
                    var succ = succs[SkipListSettings.MinLevel];

                    node.Next[SkipListSettings.MinLevel] = new MarkedReference<Node<T>>(succ, false);

                    if (!pred.Next[SkipListSettings.MinLevel].CompareAndExchange(node, false, succ, false))
                    {
                        continue;
                    }

                    for (var level = 1; level <= topLevel; level++)
                    {
                        while (true)
                        {
                            pred = preds[level];
                            succ = succs[level];

                            if (pred.Next[level].CompareAndExchange(node, false, succ, false))
                            {
                                break;
                            }

                            Find(node, ref preds, ref succs);
                        }
                    }
                    return true;
                }
            }
        }

        public bool Remove(Node<T> node)
        {
            var bottomLevel = 0;
            var preds = new Node<T>[SkipListSettings.MaxLevel + 1];
            var succs = new Node<T>[SkipListSettings.MaxLevel + 1];

            while (true)
            {
                var found = Find(node, ref preds, ref succs);
                if (!found)
                {
                    return false;
                }

                else
                {
                    Node<T> succ;
                    for (var level = node.TopLevel; level > bottomLevel; level--)
                    {
                        var isMarked = false;
                        succ = node.Next[level].Get(ref isMarked);

                        while (!isMarked)
                        {
                            node.Next[level].CompareAndExchange(succ, true, succ, false);
                            succ = node.Next[level].Get(ref isMarked);
                        }
                    }

                    var marked = false;
                    succ = node.Next[bottomLevel].Get(ref marked);

                    while (true)
                    {
                        var iMarkedIt = node.Next[bottomLevel].CompareAndExchange(succ, true, succ, false);
                        succ = succs[bottomLevel].Next[bottomLevel].Get(ref marked);

                        if (iMarkedIt)
                        {
                            Find(node, ref preds, ref succs);
                            return true;
                        }

                        if (marked)
                        {
                            return false;
                        }
                    }
                }
            }
        }
        
        private bool Find(Node<T> node, ref Node<T>[] preds, ref Node<T>[] succs)
        {
            var bottomLevel = 0;
            var marked = false;
            var isRetryNeeded = false;
            Node<T> curr = null;
            
            while (true)
            {
                var pred = _head;
                for (var level = SkipListSettings.MaxLevel; level >= bottomLevel; level--)
                {
                    curr = pred.Next[level].Value;
                    while (true)
                    {
                        var succ = curr.Next[level].Get(ref marked);
                        while (marked)
                        {
                            var snip = pred.Next[level].CompareAndExchange(succ, false, curr, false);
                            if (!snip)
                            {
                                isRetryNeeded = true;
                                break;
                            }

                            curr = pred.Next[level].Value;
                            succ = curr.Next[level].Get(ref marked);
                        }

                        if (isRetryNeeded)
                        {
                            isRetryNeeded = false;
                            continue;
                        }

                        if (curr.NodeKey < node.NodeKey)
                        {
                            pred = curr;
                            curr = succ;
                        }

                        else
                        {
                            break;
                        }
                    }

                    preds[level] = pred;
                    succs[level] = curr;
                }
                return curr != null && (curr.NodeKey == node.NodeKey);
            }
        }
    }
}
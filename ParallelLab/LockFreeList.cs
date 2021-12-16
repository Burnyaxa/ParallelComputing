using System;
using ParallelLab.Util;

namespace ParallelLab
{
    public class SkipList<T>
    {
        public Node<T> Head { get; } = new(int.MinValue);

        public Node<T> Tail { get; } = new(int.MaxValue);

        public SkipList()
        {
            for (var i = 0; i < Head.Next.Length; ++i)
            {
                Head.Next[i] = new MarkedReference<Node<T>>(Tail, false);
            }
        }

        public bool Add(Node<T> node)
        {
            var preds = new Node<T>[SkipListSettings.MaxLevel + 1];
            var succs = new Node<T>[SkipListSettings.MaxLevel + 1];

            while (true)
            {
                Find(node, ref preds, ref succs);
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

        public bool Remove(Node<T> node)
        {
            var preds = new Node<T>[SkipListSettings.MaxLevel + 1];
            var succs = new Node<T>[SkipListSettings.MaxLevel + 1];

            while (true)
            {
                var found = Find(node, ref preds, ref succs);
                if (!found)
                {
                    return false;
                }

                Node<T> succ;
                for (var level = node.TopLevel; level > SkipListSettings.MinLevel; level--)
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
                succ = node.Next[SkipListSettings.MinLevel].Get(ref marked);

                while (true)
                {
                    var iMarkedIt = node.Next[SkipListSettings.MinLevel].CompareAndExchange(succ, true, succ, false);
                    succ = succs[SkipListSettings.MinLevel].Next[SkipListSettings.MinLevel].Get(ref marked);

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

        private bool Find(Node<T> node, ref Node<T>[] preds, ref Node<T>[] succs)
        {
            var marked = false;
            var isRetryNeeded = false;
            Node<T> curr = null;

            while (true)
            {
                var pred = Head;
                for (var level = SkipListSettings.MaxLevel; level >= SkipListSettings.MinLevel; level--)
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
                            break;
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

                    if (isRetryNeeded)
                    {
                        continue;
                    }

                    preds[level] = pred;
                    succs[level] = curr;
                }

                return curr != null && (curr.NodeKey == node.NodeKey);
            }
        }
    }
}
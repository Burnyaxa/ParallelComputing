using System;
using ParallelLab.Util;
using ParallelLab.Util.Interfaces;

namespace ParallelLab
{
    public class LockFreeLinkedList<TKey, TValue> 
        where TKey : IBorderline<TValue>, new()
        where TValue : IComparable<TValue>
    {
        public LinkedListNode<TValue> Head { get; }

        public LinkedListNode<TValue> Tail { get; }

        public LockFreeLinkedList()
        {
            var dummy = new TKey();
            Head = new LinkedListNode<TValue>(dummy.MinValue());
            Tail = new LinkedListNode<TValue>(dummy.MaxValue());

            while (!Head.Next.CompareAndExchange(Tail, false, default, false))
            {
            }
        }

        public bool Add(TValue value)
        {
            var node = new LinkedListNode<TValue>(value);

            while (true)
            {
                LinkedListNode<TValue> left = null;
                var right = Search(value, ref left);
                if (right != Tail && right.Value.CompareTo(value) == 0)
                {
                    return false;
                }

                node.Next = new MarkedReference<LinkedListNode<TValue>>(right, false);
                if (left.Next.CompareAndExchange(node, false, right, false))
                {
                    return true;
                }
            }
        }

        public bool Remove(TValue value)
        {
            while (true)
            {
                LinkedListNode<TValue> left = null;
                var right = Search(value, ref left);

                if (right.Value.CompareTo(value) != 0)
                {
                    return false;
                }

                var rightNext = right.Next.Value;

                var snip = right.Next.AttemptMark(rightNext, true);
                if (!snip)
                {
                    continue;
                }

                left.Next.CompareAndExchange(right, false, rightNext, false);
                return true;
            }
        }

        public LinkedListNode<TValue> Search(TValue searchValue, ref LinkedListNode<TValue> leftNode)
        {
            var marked = false;
            retry:
            while (true)
            {
                var head = Head;
                var headNext = head.Next.Value;

                while (true)
                {
                    var succ = headNext.Next.Get(ref marked);
                    while (marked)
                    {
                        var snip = head.Next.CompareAndExchange(succ, false, headNext, false);
                        if (!snip)
                        {
                            goto retry;
                        }

                        headNext = head.Next.Value;
                        succ = headNext.Next.Get(ref marked);
                    }

                    if (headNext.Value.CompareTo(searchValue) < 0)
                    {
                        head = headNext;
                        headNext = succ;
                    }
                    else
                    {
                        leftNode = head;
                        return headNext;
                    }
                }
            }

        }
    }
}
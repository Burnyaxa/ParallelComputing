using System.Threading;

namespace ParallelLab
{
    public class LockFreeQueue<T>
    {
        private QueueNode<T> _head;
        private QueueNode<T> _tail;

        public LockFreeQueue()
        {
            _head = new QueueNode<T>(default, null);
            _tail = _head;
        }

        public void Enqueue(T value)
        {
            var node = new QueueNode<T>(value, null);

            while (true)
            {
                var tail = _tail;
                if (tail == null)
                {
                    continue;
                }
                var tailNext = tail.Next;

                if (tail != _tail)
                {
                    continue;
                }
                if (tailNext is null)
                {
                    if (Interlocked.CompareExchange(ref tail.Next, node, tailNext) != tailNext)
                    {
                        continue;
                    }
                    Interlocked.CompareExchange(ref _tail, node, tail);
                    return;
                }

                Interlocked.CompareExchange(ref _tail, tailNext, tail);
            }
        }

        public bool Dequeue(out T result)
        {
            while (true)
            {
                var head = _head;
                var tail = _tail;
                if (head == null)
                {
                    continue;
                }
                var next = head.Next;

                if (head != _head) continue;
                if (next is null)
                {
                    result = default;
                    return false;
                }

                if (head == tail)
                {
                    Interlocked.CompareExchange(ref _tail, next, tail);
                }
                else
                {
                    result = next.Value;
                    if (Interlocked.CompareExchange(ref _head, next, head) == head)
                    {
                        return true;
                    }
                }
            }
        }
    }
}
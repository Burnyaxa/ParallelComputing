using ParallelLab.Util;

namespace ParallelLab
{
    public class LinkedListNode<T>
    {
        public T Value { get; set; }

        public MarkedReference<LinkedListNode<T>> Next { get; set; }

        public LinkedListNode(T value)
        {
            Value = value;
            Next = new MarkedReference<LinkedListNode<T>>(default, false);
        }
        
    }
}
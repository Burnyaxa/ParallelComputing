using ParallelLab.Util;

namespace ParallelLab
{
    public class LinkedListNode<T>
    {
        private T _value;
        private MarkedReference<LinkedListNode<T>> _next;
        public T Value
        {
            get => _value;
            set => _value = value;
        }
        public MarkedReference<LinkedListNode<T>> Next
        {
            get => _next;
            set => _next = value;
        }

        public LinkedListNode(T value)
        {
            _value = value;
            _next = new MarkedReference<LinkedListNode<T>>(default, false);
        }
        
    }
}
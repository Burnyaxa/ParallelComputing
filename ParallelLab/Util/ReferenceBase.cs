using ParallelLab.Util.Atomic;

namespace ParallelLab.Util
{
    public class ReferenceBase<T>
    {
        public readonly T Value;
        public readonly AtomicBool Marked;

        public ReferenceBase(T value, bool marked)
        {
            this.Value = value;
            this.Marked = new AtomicBool(marked);
        }
    }
}
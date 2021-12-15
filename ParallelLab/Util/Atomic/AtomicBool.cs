using System.Threading;

namespace ParallelLab.Util.Atomic
{
    public class AtomicBool
    {
        private const int ValueTrue = 1;
        private const int ValueFalse = 0;

        private int _currentValue;

        public AtomicBool(bool initialValue)
        {
            _currentValue = BoolToInt(initialValue);
        }

        private int BoolToInt(bool value)
        {
            return value ? ValueTrue : ValueFalse;
        }

        private bool IntToBool(int value)
        {
            return value == ValueTrue;
        }

        public bool Value =>
            IntToBool(Interlocked.Add(
                ref _currentValue, 0));

        public bool SetValue(bool newValue)
        {
            return IntToBool(
                Interlocked.Exchange(ref _currentValue,
                    BoolToInt(newValue)));
        }

        public bool CompareAndSet(bool expectedValue,
            bool newValue)
        {
            var expectedVal = BoolToInt(expectedValue);
            var newVal = BoolToInt(newValue);
            return (Interlocked.CompareExchange(
                ref _currentValue, newVal, expectedVal) == expectedVal);
        }
    }
}
namespace ParallelLab.Util.Interfaces
{
    public interface IBorderline<T>
    {
        T MinValue();
        T MaxValue();
        T Value { get; set; }
    }
}
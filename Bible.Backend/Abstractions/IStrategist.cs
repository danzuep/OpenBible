namespace Bible.Backend.Abstractions
{
    public interface IStrategist<TIn, TOut>
    {
        TOut? ChooseStrategy(TIn input);
    }
}
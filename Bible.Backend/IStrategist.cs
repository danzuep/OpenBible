namespace Bible.Backend
{
    public interface IStrategist<TIn, TOut>
    {
        TOut? ChooseStrategy(TIn input);
    }
}
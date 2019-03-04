namespace VSS.TRex.Common.RequestStatistics
{
  public struct StatisticsElement
  {
    /// <summary>
    /// Value of the statistics elements being tracked
    /// </summary>
    private long value;

    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public long Value => value;

    /// <summary>
    /// Thread safe incrementer for the statistics element
    /// </summary>
    public void Increment() => System.Threading.Interlocked.Increment(ref value);
  }
}

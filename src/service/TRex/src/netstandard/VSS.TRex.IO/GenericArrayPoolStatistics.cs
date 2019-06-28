namespace VSS.TRex.IO
{
  public struct GenericArrayPoolStatistics
  {
    /// <summary>
    /// THe index in the main pools array this pool array occupies
    /// </summary>
    public int PoolIndex;

    /// <summary>
    /// The total element capacity for this pool array
    /// </summary>
    public int PoolCapacity;

    /// <summary>
    /// The current number of elements in the pool available for rent within the pool array
    /// </summary>
    public int AvailCount;

    /// <summary>
    /// The maximum number of observed outstanding rentals against this pool array
    /// </summary>
    public int HighWaterRents;

    /// <summary>
    /// The current number of elements on issue from this pool.
    /// </summary>
    public int CurrentRents;
  }
}

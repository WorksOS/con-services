using System;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.Events.Models
{
  /// <summary>
  /// Stores a override value and the end date used for overriding on machine events
  /// </summary>
  public struct OverrideEvent<T>
  {
    /// <summary>
    /// The end of the time period that is overridden. The start time is stored in the event.
    /// </summary>
    public DateTime EndDate;
    /// <summary>
    /// The override value (layer ID or design ID).
    /// </summary>
    public T Value;

    public OverrideEvent(DateTime endDate, T value)
    {
      EndDate = endDate;
      Value = value;
    }

    public static OverrideEvent<T> Null(T nullValue)
    {
      return new OverrideEvent<T>(CellPassConsts.NullTime, nullValue);
    }
  }
}

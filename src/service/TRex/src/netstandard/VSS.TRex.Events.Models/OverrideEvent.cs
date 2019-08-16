using System;

namespace VSS.TRex.Events.Models
{
  /// <summary>
  /// Stores a override value and the end date used for overriding on machine events
  /// </summary>
  public struct  OverrideEvent<T>
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

    //TODO: Work out what the 'null' datetime is. Null design ID is Consts.kNoDesignNameID and null layer ID is CellEvents.NullLayerID.
    public static OverrideEvent<T> Null()
    {
      return new OverrideEvent<T>();
    }
  }
}

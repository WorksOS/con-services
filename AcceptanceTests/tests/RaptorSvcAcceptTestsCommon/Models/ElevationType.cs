namespace RaptorSvcAcceptTestsCommon.Models
{
  /// <summary>
  /// Controls the source of information from cells for queries depending on their elevation
  /// This is copied from ...\RaptorServicesCommon\Models\ElevationType.cs
  /// </summary>
  public enum ElevationType
  {
    /// <summary>
    /// Choose the elevation from the last recorded cell pass that meets the filter criteria
    /// </summary>
    Last = 0,

    /// <summary>
    /// Choose the elevation from the first recorded cell pass that meets the filter criteria
    /// </summary>
    First = 1,

    /// <summary>
    /// Choose the highest elevation of the cell passes that meet the filter criteria
    /// </summary>
    Highest = 2,

    /// <summary>
    /// Choose the lowest elevation of the cell passes that meet the filter criteria
    /// </summary>
    Lowest = 3
  }
}
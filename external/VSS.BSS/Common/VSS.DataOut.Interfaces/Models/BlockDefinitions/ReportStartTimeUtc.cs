using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions
{
  /// <summary>
  /// Report Start Time
  /// The report start time in UTC format.
  /// </summary>
  [Serializable]
  public class ReportStartTimeUtc : Block
  {
    public DateTime Value { get; set; }
  }
}

using System;

namespace VSS.TRex.Types.Types
{
  /// <summary>
  /// Specifies CCV summarize mode. 
  /// </summary>
  [Flags]
  public enum CCVSummaryTypes
  {
    /// <summary>
    /// Null value - no CCV summary types set
    /// </summary>
    None = 0,
    /// <summary>
    /// Summarize by compaction
    /// </summary>
    Compaction = 1,
    /// <summary>
    /// Summarize by thickness
    /// </summary>
    Thickness = 2,
    /// <summary>
    /// Summarize by work in progress
    /// </summary>
    WorkInProgress = 4
  }
}

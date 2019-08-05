using System;

namespace VSS.TRex.Types.Types
{
  /// <summary>
  /// Specifies MDP summarize mode. 
  /// </summary>
  [Flags]
  public enum MDPSummaryTypes
  {
    /// <summary>
    /// Null value - no MDP summary types set
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

using System;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.TAGFiles.Models
{
  /// <summary>
  /// Represents the state of a TAG file stored in the TAG file buffer queue awaiting processing.
  /// </summary>
  public class SegmentRetirementQueueItem
  {
    /// <summary>
    /// The date at which the segment to be retired was inserted into the buffer queue. 
    /// </summary>
    public DateTime InsertUTC;

    /// <summary>
    /// The key of the segment stream to be retired
    /// </summary>
    public ISubGridSpatialAffinityKey SegmentKey;
  }
}

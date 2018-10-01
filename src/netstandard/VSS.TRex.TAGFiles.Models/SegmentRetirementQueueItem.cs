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
    /// The project this segment retirement queue item refers to
    /// </summary>
    public Guid ProjectUID;

    /// <summary>
    /// The date at which the segment to be retired was inserted into the buffer queue. 
    /// </summary>
    public DateTime InsertUTC;

    /// <summary>
    /// The list of keys of the subgrid and segment streams to be retired.
    /// This list is submitted as a single collection of retirement items per integration update epoch in the TAG file processor
    /// </summary>
    public ISubGridSpatialAffinityKey[] SegmentKeys;
  }
}

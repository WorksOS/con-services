using System;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.TAGFiles.Models
{
  /// <summary>
  /// Represents a segment that has been stored in the persistent layer as a result on TAG file processing that
  /// has subsequently been updated with a later TAG file generated update.
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
//    [QuerySqlField(IsIndexed = true)]
    public long InsertUTCAsLong;

    /// <summary>
    /// The list of keys of the subgrid and segment streams to be retired.
    /// This list is submitted as a single collection of retirement items per integration update epoch in the TAG file processor
    /// </summary>
    public ISubGridSpatialAffinityKey[] SegmentKeys;
  }
}

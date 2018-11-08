using System;
using System.Collections.Generic;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
  public interface ISegmentRetirementQueue
  {
    void Add(ISegmentRetirementQueueKey key, SegmentRetirementQueueItem value);

    /// <summary>
    /// Finds all the items in the retirement queue ready for removal and returns them
    /// </summary>
    /// <param name="earlierThan"></param>
    /// <returns></returns>
    List<SegmentRetirementQueueItem> Query(DateTime earlierThan);
  }
}
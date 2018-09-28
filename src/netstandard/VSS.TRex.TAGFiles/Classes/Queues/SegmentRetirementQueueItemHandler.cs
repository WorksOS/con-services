using System.Collections.Generic;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
  public class SegmentRetirementQueueItemHandler
  {
    /// <summary>
    /// Takes a set of segment retirees and removes them from grid storage. Once items are successfully removed from
    /// storage they are removed from the retirement queue
    /// </summary>
    /// <param name="retirees"></param>
    public void Process(IEnumerable<SegmentRetirementQueueItem> retirees)
    {
      // ....
    }
  }
}

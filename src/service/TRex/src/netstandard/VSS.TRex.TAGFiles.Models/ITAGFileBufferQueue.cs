using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.TAGFiles.Models
{
  public interface ITAGFileBufferQueue
  {
    bool Add(ITAGFileBufferQueueKey key, TAGFileBufferQueueItem value);
  }
}

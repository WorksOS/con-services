using System;
using System.Threading;
using VSS.TRex.DI;
using VSS.TRex.IO;

namespace VSS.TRex.Common
{
  public class RecycledMemoryStreamHeartBeatLogger
  {
    private static RecyclableMemoryStreamManager _manager = DIContext.Obtain<RecyclableMemoryStreamManager>();

    private long _blockCreationCount = 0;
    private long _streamCreationCount = 0;
    private long _streamDisposedCount = 0;
    private long _streamFinalizedCount = 0;

    public override string ToString()
    {
      return $"Streams created/disposed/finalized: {_streamCreationCount}/{_streamDisposedCount}/{_streamFinalizedCount} Num blocks current/created: {_manager.SmallPoolInUseSize / _manager.BlockSize}/{_blockCreationCount}, Small block memory in use/free: {(1.0 * _manager.SmallPoolInUseSize)/1e6:F3}Mb/{(1.0 * _manager.SmallPoolFreeSize)/1e6:F3}Mb";
    }

    public RecycledMemoryStreamHeartBeatLogger()
    {
      _manager.BlockCreated += () => { Interlocked.Increment(ref _blockCreationCount); };
      _manager.StreamCreated += () => { Interlocked.Increment(ref _streamCreationCount); };
      _manager.StreamDisposed += () => { Interlocked.Increment(ref _streamDisposedCount); };
      _manager.StreamFinalized += () => { Interlocked.Increment(ref _streamFinalizedCount); };
    }
  }
}

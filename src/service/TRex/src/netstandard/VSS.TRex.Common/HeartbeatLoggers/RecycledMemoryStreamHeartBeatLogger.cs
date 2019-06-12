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

    public override string ToString()
    {
      return $"Num blocks current/created: {_manager.SmallPoolInUseSize / _manager.BlockSize}/{_blockCreationCount}, Small block memory in use/free: {(1.0 * _manager.SmallPoolInUseSize)/1e6:F3}Mb/{(1.0 * _manager.SmallPoolFreeSize)/1e6:F3}Mb";
    }

    public RecycledMemoryStreamHeartBeatLogger()
    {
      _manager.BlockCreated += () => { Interlocked.Increment(ref _blockCreationCount); };
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.Server.PSNode
{
  public class MemoryHeartBeatLogger
  {
    public override string ToString()
    {
      return $"Total managed memory use: {(1.0 * GC.GetTotalMemory(false))/1e6:F3}Mb";
    }
  }
}

using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;

namespace VSS.TRex.Common
{
  public class MemoryHeartBeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<MemoryHeartBeatLogger>();

    public void HeartBeat()
    {
      Log.LogInformation("Heartbeat: " + ToString());
    }

    public override string ToString()
    {
      return $"Total managed memory use: {(1.0 * GC.GetTotalMemory(false)) / 1e6:F3}Mb";
    }
  }
}

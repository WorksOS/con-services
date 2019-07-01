using System;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.IO;

namespace VSS.TRex.Cells
{
  public class GenericTwoDArrayCacheRegisterHeartBeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<GenericTwoDArrayCacheRegisterHeartBeatLogger>();

    private readonly StringBuilder sb = new StringBuilder();

    public void HeartBeat()
    {
      try
      {
        foreach (var cache in GenericTwoDArrayCacheRegister.ArrayPoolCaches)
        {
          var stats = cache?.Statistics();

          if (stats.HasValue)
          {
            sb.Clear();
            sb.Append(cache.TypeName());
            sb.AppendLine("-2DArrayCache: Size/Max/WaterMark/HighWaterMark: ");
            sb.AppendLine($"{stats.Value.currentSize}/{stats.Value.maxSize}/{stats.Value.currentWaterMark}/{stats.Value.highWaterMark}");

            Log.LogInformation(sb.ToString());
          }
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred during heart beat log epoch");
      }
    }
  }
}

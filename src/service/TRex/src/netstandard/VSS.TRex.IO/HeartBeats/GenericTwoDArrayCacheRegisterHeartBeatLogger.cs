using System;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;

namespace VSS.TRex.IO.Heartbeats
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
          var stats = cache.Statistics();

          sb.Clear();
          sb.Append(cache.TypeName());
          sb.Append("-2DArrayCache: Size/Max/NumCreated/WaterMark/HighWaterMark: ");
          sb.Append($"{stats.CurrentSize}/{stats.MaxSize}/{stats.NumCreated}/{stats.CurrentWaterMark}/{stats.HighWaterMark}");

          Log.LogInformation(sb.ToString());
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred during heart beat log epoch");
      }
    }
  }
}

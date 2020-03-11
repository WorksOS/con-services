using System;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.DI;

namespace VSS.TRex.IO.Heartbeats
{
  public class GenericTwoDArrayCacheRegisterHeartBeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<GenericTwoDArrayCacheRegisterHeartBeatLogger>();

    private readonly StringBuilder _sb = new StringBuilder();

    public void HeartBeat()
    {
      try
      {
        if (!DIContext.Obtain<IConfigurationStore>().GetValueBool("HEARTBEAT_LOGGING_ENABLED_GenericTwoDArrayCache", false))
        {
          return;
        }

        foreach (var cache in GenericTwoDArrayCacheRegister.ArrayPoolCaches)
        {
          var stats = cache.Statistics();

          _sb.Clear();
          _sb.Append(cache.TypeName());
          _sb.Append("-2DArrayCache: Size/Max/NumCreated/WaterMark/HighWaterMark: ");
          _sb.Append($"{stats.CurrentSize}/{stats.MaxSize}/{stats.NumCreated}/{stats.CurrentWaterMark}/{stats.HighWaterMark}");

          Log.LogInformation(_sb.ToString());
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred during heart beat log epoch");
      }
    }
  }
}

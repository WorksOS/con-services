using System;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.DI;

namespace VSS.TRex.IO.Heartbeats
{
  public class GenericArrayPoolRegisterHeartBeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<GenericArrayPoolRegisterHeartBeatLogger>();

    private readonly StringBuilder _sb = new StringBuilder();

    public void HeartBeat()
    {
      try
      {
        if (!DIContext.Obtain<IConfigurationStore>().GetValueBool("HEARTBEAT_LOGGING_ENABLED_GenericArrayPoolRegisterHeartBeatLogger", false))
        {
          return;
        }

        foreach (var cache in GenericArrayPoolCachesRegister.ArrayPoolCaches)
        {
          var stats = cache.Statistics();

          if (stats != null)
          {
            _sb.Clear();
            _sb.Append(cache.TypeName());
            _sb.Append("-ArrayPool: Index/Capacity/MaxRented/Rented/Available: ");

            foreach (var stat in stats)
            {
              _sb.Append($"{stat.PoolIndex}/{stat.PoolCapacity}/{stat.HighWaterRents}/{stat.CurrentRents}/{stat.AvailCount} | ");
            }

            Log.LogInformation("Heartbeat: " + _sb);
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

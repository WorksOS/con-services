using System;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.IO;

namespace VSS.TRex.Cells
{
  public class GenericArrayPoolRegisterHeartBeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<GenericArrayPoolRegisterHeartBeatLogger>();

    private readonly StringBuilder sb = new StringBuilder();

    public void HeartBeat()
    {
      try
      {
        foreach (var cache in GenericArrayPoolCachesRegister.ArrayPoolCaches)
        {
          var stats = cache.Statistics();

          if (stats != null)
          {
            sb.Clear();
            sb.Append(cache.TypeName());
            sb.Append("-ArrayPool: Index/Capacity/MaxRented/Rented/Available: ");

            foreach (var stat in stats)
            {
              sb.Append($"{stat.PoolIndex}/{stat.PoolCapacity}/{stat.HighWaterRents}/{stat.CurrentRents}/{stat.AvailCount}");
            }

            Log.LogInformation("Heartbeat: " + sb);
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

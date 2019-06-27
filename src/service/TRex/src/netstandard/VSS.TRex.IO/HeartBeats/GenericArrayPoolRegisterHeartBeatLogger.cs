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
            sb.AppendLine("-ArrayPool: Index/Capacity/Available/Rented: ");

            foreach (var stat in stats)
            {
              sb.AppendLine($"{stat.poolIndex}/{stat.poolCapacity}/{stat.poolCapacity - stat.rentalCount}/{stat.rentalCount}");
            }

            Log.LogInformation("Heartbeat: " + sb.ToString());
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

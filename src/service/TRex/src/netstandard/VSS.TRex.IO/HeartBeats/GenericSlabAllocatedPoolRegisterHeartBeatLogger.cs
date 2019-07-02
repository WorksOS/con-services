using System;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;

namespace VSS.TRex.IO.Heartbeats
{
  public class GenericSlabAllocatedPoolRegisterHeartBeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<GenericSlabAllocatedPoolRegisterHeartBeatLogger>();

    private readonly StringBuilder sb = new StringBuilder();

    public void HeartBeat()
    {
      try
      {
        foreach (var cache in GenericSlabAllocatedArrayPoolRegister.ArrayPoolCaches)
        {
          var stats = cache.Statistics();

          if (stats != null)
          {
            sb.Clear();
            sb.Append(cache.TypeName());
            sb.Append("-SlabAllocatedArrayPool: Index/ArraySize/Capacity/Available/Rented: ");

            foreach (var stat in stats)
            {
              sb.Append($"{stat.PoolIndex}/{stat.ArraySize}/{stat.Capacity}/{stat.Capacity - stat.RentedItems}/{stat.RentedItems} | ");
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

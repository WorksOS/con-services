using System;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.DI;

namespace VSS.TRex.IO.Heartbeats
{
  public class GenericSlabAllocatedPoolRegisterHeartBeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<GenericSlabAllocatedPoolRegisterHeartBeatLogger>();

    private readonly StringBuilder _sb = new StringBuilder();

    public void HeartBeat()
    {
      try
      {
        if (!DIContext.Obtain<IConfigurationStore>().GetValueBool("HEARTBEAT_LOGGING_ENABLED_GenericSlabAllocatedPoolRegisterHeartBeatLogger", false))
        {
          return;
        }

        foreach (var cache in GenericSlabAllocatedArrayPoolRegister.ArrayPoolCaches)
        {
          var stats = cache.Statistics();

          if (stats != null)
          {
            _sb.Clear();
            _sb.Append(cache.TypeName());
            _sb.Append("-SlabAllocatedArrayPool: Index/ArraySize/Capacity/Available/Rented: ");

            foreach (var stat in stats)
            {
              _sb.Append($"{stat.PoolIndex}/{stat.ArraySize}/{stat.Capacity}/{stat.Capacity - stat.RentedItems}/{stat.RentedItems} | ");
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

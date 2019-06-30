using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.IO.Helpers;

namespace VSS.TRex.Cells
{
  public class SlabAllocatedCellPassArrayPoolHeartBeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SlabAllocatedCellPassArrayPoolHeartBeatLogger>();

    private readonly StringBuilder sb = new StringBuilder();

    public void HeartBeat()
    {
      Log.LogInformation("Heartbeat: " + ToString());
    }

    public override string ToString()
    {
      var stats = SlabAllocatedArrayPoolHelper<CellPass>.Caches?.Statistics();

      if (stats != null)
      {
        sb.Clear();
        sb.AppendLine("SlabAllocatedCellPassArrayPool: Index/ArraySize/Capacity/Available/Rented: ");

        foreach (var stat in stats)
        {
          sb.AppendLine($"{stat.poolIndex}/{stat.arraySize}/{stat.capacity}/{stat.capacity - stat.rentedItems}/{stat.rentedItems}");
        }

        return sb.ToString();
      }

      return "SlabAllocatedCellPassArrayPoolHelper caches not available yet.";
    }
  }
}

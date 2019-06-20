using System.Text;
using VSS.TRex.Cells.Helpers;

namespace VSS.TRex.Cells
{
  public class SlabAllocatedCellPassArrayPoolHeartBeatLogger
  {
    private readonly StringBuilder sb = new StringBuilder();

    public override string ToString()
    {
      var stats = SlabAllocatedCellPassArrayPoolHelper.Caches?.Statistics();

      if (stats != null)
      {
        sb.Clear();
        sb.AppendLine("SlabAllocatedCellPassArrayPool: Index/ArraySize/Capacity/Available/Rented: ");

        foreach (var stat in stats)
        {
          sb.AppendLine($"{stat.poolIndex}/{stat.arraySize}/{stat.capacity}/{stat.availableItems}/{stat.capacity - stat.availableItems}");
        }

        return sb.ToString();
      }

      return "SlabAllocatedCellPassArrayPoolHelper caches not available yet.";
    }
  }
}

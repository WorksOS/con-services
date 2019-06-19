using System.Text;
using VSS.TRex.Cells.Helpers;

namespace VSS.TRex.Cells
{
  public class SlabAllocatedCellPassArrayPoolHeartBeatLogger
  {
    private StringBuilder sb = new StringBuilder();

    public string ToString()
    {
      var stats = SlabAllocatedCellPassArrayPoolHelper.Caches.Statistics();

      sb.Clear();
      foreach (var stat in stats)
      {
        sb.AppendLine($"Index: {stat.poolIndex}, ArraySize: {stat.arraySize}, Capacity: {stat.capacity}, Available: {stat.availableItems}");
      }

      return sb.ToString();
    }
  }
}

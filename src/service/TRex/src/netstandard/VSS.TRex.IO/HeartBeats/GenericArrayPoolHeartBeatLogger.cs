using System.Text;
using VSS.TRex.IO.Helpers;

namespace VSS.TRex.Cells
{
  public class GenericArrayPoolHeartBeatLogger<T>
  {
    private readonly StringBuilder sb = new StringBuilder();
    private readonly string _typePrefix = typeof(T).Name;

    public override string ToString()
    {
      var stats = GenericArrayPoolCacheHelper<T>.Caches?.Statistics();

      if (stats != null)
      {
        sb.Clear();
        sb.Append(_typePrefix);
        sb.AppendLine("-ArrayPool: Index/Capacity/Available/Rented: ");

        foreach (var stat in stats)
        {
          sb.AppendLine($"{stat.poolIndex}/{stat.poolCapacity}/{stat.poolCapacity - stat.rentalCount}/{stat.rentalCount}");
        }

        return sb.ToString();
      }

      return $"{_typePrefix}-ArrayPool caches not available yet.";
    }
  }
}

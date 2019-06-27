using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.IO.Helpers;

namespace VSS.TRex.Cells
{
  public class GenericTwoDArrayCacheHeartBeatLogger<T> : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<GenericTwoDArrayCacheHeartBeatLogger<T>>();

    private readonly StringBuilder sb = new StringBuilder();
    private readonly string _typePrefix = typeof(T).Name;

    public void HeartBeat()
    {
      Log.LogInformation("Heartbeat: " + ToString());
    }

    public override string ToString()
    {
      var stats = GenericTwoDArrayCacheHelper<T>.Caches?.Statistics();

      if (stats.HasValue)
      {
        sb.Clear();
        sb.Append(_typePrefix);
        sb.AppendLine("-2DArrayCache: Size/Max: ");
        sb.AppendLine($"{stats.Value.currentSize}/{stats.Value.maxSize}");

        return sb.ToString();
      }

      return $"{_typePrefix}-2DArrayCache cache not available yet.";
    }
  }
}

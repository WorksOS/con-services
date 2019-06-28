using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SiteModels
{
  /// <summary>
  /// Provides heartbeat logging on the aggregate state of site models present in memory
  /// </summary>
  public class SiteModelsHeartBeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModelsHeartBeatLogger>();

    public void HeartBeat()
    {
      Log.LogInformation("Heartbeat: " + ToString());
    }

    public override string ToString()
    {
      var sw = Stopwatch.StartNew();

      // Get the list of site models
      var siteModels = DIContext.Obtain<ISiteModels>().GetSiteModels();

      var sumSubGridsInCache = siteModels.Sum(x => x.Grid.CountLeafSubGridsInMemory());
      var subSegmentsInCache = siteModels.Sum(x =>
      {
        var count = 0;

        x.Grid.Root.ScanSubGrids(x.Grid.FullCellExtent(), 
          s => {
            count += ((IServerLeafSubGrid)s).Directory.SegmentDirectory.Sum(sd => sd.Segment == null ? 0 : 1);
            return true;
          });

        return count;
      });

      return $"#Models in cache: {siteModels.Count}, total sub grids/segments in memory: {sumSubGridsInCache}/{subSegmentsInCache} [Elapsed = {sw.Elapsed}]";
    }
  }
}

using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;

namespace VSS.TRex.SiteModels.Heartbeats
{
  public class SiteModelRebuilderHeartbeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SiteModelRebuilderHeartbeatLogger>();

    public void HeartBeat()
    {
      _log.LogInformation("Heartbeat: " + ToString());
    }

    private string PhaseState(IRebuildSiteModelMetaData metaData)
    {
      return metaData.Phase switch
      {
        RebuildSiteModelPhase.Unknown => "",
        RebuildSiteModelPhase.Deleting => $"Selectivity: {metaData.DeletionSelectivity}",
        RebuildSiteModelPhase.Scanning => $"Scanned: Collections={metaData.NumberOfTAGFileKeyCollections}, Files={metaData.NumberOfTAGFilesFromS3}",
        RebuildSiteModelPhase.Submitting => $"Submitted files: {metaData.NumberOfTAGFilesSubmitted}",
        RebuildSiteModelPhase.Monitoring => $"Processed files: {metaData.NumberOfTAGFilesProcessed}, last processed: {metaData.LastProcessedTagFile}",
        RebuildSiteModelPhase.Completion => $"Result: {metaData.RebuildResult}",
        RebuildSiteModelPhase.Complete => "",
        _ => "Unknown phase for state logging"
      };
    }

    public override string ToString()
    {
      var manager = DIContext.Obtain<ISiteModelRebuilderManager>();

      if (manager == null)
      {
        return "No site model rebuilder manager available";
      }

      var sb = new StringBuilder();
      sb.Append("Number of projects being rebuilt: ").Append(manager.RebuildCount()).Append(": ");

      if (manager.RebuildCount() > 0)
        sb.AppendJoin(", ", manager.GetRebuildersState().Select(x => $"{x.ProjectUID} - {x.Phase} - {PhaseState(x)}"));

      return sb.ToString();
    }
  }
}

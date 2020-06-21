using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.Common.Records;
using VSS.TRex.DI;
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

    public override string ToString()
    {
      var manager = DIContext.Obtain<ISiteModelRebuilderManager>();

      var sb = new StringBuilder();
      sb.Append("Number of projects being rebuilt: ").Append(manager.RebuildCount());

      if (manager.RebuildCount() > 0)
        sb.AppendJoin(", ", manager.GetRebuilersState().Select(x => $"{x.ProjectUID} - {x.Phase}"));

      return sb.ToString();
    }
  }
}

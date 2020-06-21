using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;
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

      return $"Number of projects being rebuilt {manager.RebuildCount()}";
    }
  }
}

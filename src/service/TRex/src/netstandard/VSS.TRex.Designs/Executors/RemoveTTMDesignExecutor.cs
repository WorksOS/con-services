using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Designs.Executors
{
  public class RemoveTTMDesignExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<RemoveTTMDesignExecutor>();

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    public DesignProfilerRequestResult Execute(Guid projectUid, Guid designUid)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false);

        if (siteModel == null)
        {
          _log.LogError($"Site model {projectUid} not found");
          return DesignProfilerRequestResult.NoSelectedSiteModel;
        }

        var removed = DIContext.Obtain<IDesignManager>().Remove(projectUid, designUid);

        if (!removed)
        {
          _log.LogError($"Failed to remove design {designUid} from project {projectUid} as it may not exist in the project");
          return DesignProfilerRequestResult.DesignDoesNotExist;
        }

        return DesignProfilerRequestResult.OK;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Execute: Exception: ");
        return DesignProfilerRequestResult.UnknownError;
      }
    }
  }
}

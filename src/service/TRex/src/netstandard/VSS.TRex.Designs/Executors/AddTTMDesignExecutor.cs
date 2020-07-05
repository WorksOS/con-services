using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.Executors
{
  public class AddTTMDesignExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AddTTMDesignExecutor>();

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    public IDesign Execute(Guid projectUid, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents, ISubGridTreeBitMask existenceMap)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false);

        if (siteModel == null)
        {
          _log.LogError($"Site model {projectUid} not found");
          return null;
        }

        var design = DIContext.Obtain<IDesignManager>().Add(projectUid, designDescriptor, extents, existenceMap);

        if (design == null)
        {
          _log.LogError($"Failed to add design file {designDescriptor} to project {projectUid}");
        }

        return design;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Execute: Exception: ");
        return null;
      }
    }
  }
}

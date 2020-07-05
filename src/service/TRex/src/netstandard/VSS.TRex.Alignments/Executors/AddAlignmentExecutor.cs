using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Alignments.Executors
{
  public class AddAlignmentExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AddAlignmentExecutor>();

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    public IAlignment Execute(Guid projectUid, DesignDescriptor designDescriptor, BoundingWorldExtent3D extents)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false);

        if (siteModel == null)
        {
          _log.LogError($"Site model {projectUid} not found");
          return null;
        }

        var alignment = DIContext.Obtain<IAlignmentManager>().Add(projectUid, designDescriptor, extents);

        if (alignment == null)
        {
          _log.LogError($"Failed to add design file {designDescriptor} to project {projectUid}");
        }

        return alignment;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Execute: Exception: ");
        return null;
      }
    }
  }
}

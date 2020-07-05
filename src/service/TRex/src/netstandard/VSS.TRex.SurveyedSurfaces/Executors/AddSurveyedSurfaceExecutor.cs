using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SurveyedSurfaces.Executors
{
  public class AddSurveyedSurfaceExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AddSurveyedSurfaceExecutor>();

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    public ISurveyedSurface Execute(Guid projectUid, DesignDescriptor designDescriptor, DateTime asAtDate, BoundingWorldExtent3D extents, ISubGridTreeBitMask existenceMap)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false);

        if (siteModel == null)
        {
          _log.LogError($"Site model {projectUid} not found");
          return null;
        }

        var surveyedSurface = DIContext.Obtain<ISurveyedSurfaceManager>().Add(projectUid, designDescriptor, asAtDate, extents, existenceMap);

        if (surveyedSurface == null)
        {
          _log.LogError($"Failed to add surveyed surface file {designDescriptor}, asAtDate {asAtDate} to project {projectUid}");
        }

        return surveyedSurface;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Execute: Exception: ");
        return null;
      }
    }
  }
}

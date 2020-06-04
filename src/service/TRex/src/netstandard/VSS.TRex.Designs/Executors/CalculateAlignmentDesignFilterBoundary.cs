using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateAlignmentDesignFilterBoundary
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateAlignmentDesignFilterBoundary>();

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    /// <returns></returns>
    public Fence Execute(Guid projectUid, Guid referenceDesignUid, double startStation, double endStation, double leftOffset, double rightOffset)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false);
        var design = DIContext.Obtain<IDesignFiles>()?.Lock(referenceDesignUid, projectUid, siteModel.CellSize, out var lockResult);

        if (design == null)
        {
          Log.LogWarning($"Failed to read file for design {referenceDesignUid}");
          return null;
        }

        if (design is SVLAlignmentDesign svlDesign)
        {
          var result = svlDesign.DetermineFilterBoundary(startStation, endStation, leftOffset, rightOffset, out var fence);

          if (result != DesignProfilerRequestResult.OK)
          {
            Log.LogWarning($"Failed to compute filter boundary with error {result}");
            return null;
          }

          return fence;
        }

        throw new TRexException($"Design {design.FileName} is not an alignment design");
      }
      catch (Exception E)
      {
        Log.LogError(E, "Execute: Exception: ");
        return null;
      }
    }
  }
}

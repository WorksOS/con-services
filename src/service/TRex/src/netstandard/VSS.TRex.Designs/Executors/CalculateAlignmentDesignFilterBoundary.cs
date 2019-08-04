using System;
using Microsoft.Extensions.Logging;
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
    public Fence Execute(Guid projectUID, Guid referenceDesignUID, double startStation, double endStation, double leftOffset, double rightOffset)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUID, false);
        var design = DIContext.Obtain<IDesignFiles>()?.Lock(referenceDesignUID, projectUID, siteModel.CellSize, out var lockResult);

        if (design == null)
        {
          Log.LogWarning($"Failed to read file for design {referenceDesignUID}");
          return null;
        }

        var result = (design as SVLAlignmentDesign).DetermineFilterBoundary(startStation, endStation, leftOffset, rightOffset, out var fence);

        if (result != DesignProfilerRequestResult.OK)
        {
          Log.LogWarning($"Failed to compute filter boundary with error {result}");
          return null;
        }

        return fence;
      }
      catch (Exception E)
      {
        Log.LogError(E, "Execute: Exception: ");
        return null;
      }
    }
  }
}

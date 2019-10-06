using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateAlignmentDesignStationRange
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateAlignmentDesignStationRange>();

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    /// <param name="projectUID"></param>
    /// <param name="referenceDesignUID"></param>
    /// <returns></returns>
    public (double StartStation, double EndStation) Execute(Guid projectUID, Guid referenceDesignUID)
    {
      try
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUID, false);
        var design = DIContext.Obtain<IDesignFiles>()?.Lock(referenceDesignUID, projectUID, siteModel.CellSize, out var lockResult);

        if (design == null)
        {
          Log.LogWarning($"Failed to read file for design {referenceDesignUID}");
          return (double.MaxValue, double.MinValue);
        }

        return (design as SVLAlignmentDesign).GetStationRange();
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Failed to compute alignment design station station range. Site Model ID: {projectUID} design ID: {referenceDesignUID}");
        return (double.MaxValue, double.MinValue);
      }
    }
  }
}

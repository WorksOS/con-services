using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces.Interfaces;
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
    public (double StartStation, double EndStation) Execute(ISiteModelBase siteModel, Guid referenceDesignUid)
    {
      try
      {
        if (siteModel == null)
        {
          return (double.MaxValue, double.MinValue);
        }
        var design = DIContext.ObtainRequired<IDesignFiles>()?.Lock(referenceDesignUid, siteModel, siteModel.CellSize, out var lockResult);

        if (design == null)
        {
          Log.LogWarning($"Failed to read file for design {referenceDesignUid}");
          return (double.MaxValue, double.MinValue);
        }

        if (design is SVLAlignmentDesign svlDesign)
        {
          return svlDesign.GetStationRange();
        }

        throw new TRexException($"Design {design.FileName} is not an alignment design");
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Failed to compute alignment design station station range. Site Model ID: {siteModel.ID} design ID: {referenceDesignUid}");
        return (double.MaxValue, double.MinValue);
      }
    }
  }
}

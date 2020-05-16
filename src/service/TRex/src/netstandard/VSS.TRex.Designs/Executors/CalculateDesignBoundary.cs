using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateDesignBoundary
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignBoundary>();

    private IDesignFiles designs;

    private IDesignFiles Designs => designs ??= DIContext.Obtain<IDesignFiles>();

    /// <summary>
    /// Default no-args constructor
    /// </summary>
    public CalculateDesignBoundary()
    {
    }

    /// <summary>
    /// Performs the donkey work of the boundary calculation
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="calcResult"></param>
    /// <returns></returns>
    private List<Fence> Calc(DesignBoundaryArgument arg, out DesignProfilerRequestResult calcResult)
    {
      calcResult = DesignProfilerRequestResult.UnknownError;

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID, false);

      var design = Designs.Lock(arg.ReferenceDesign.DesignID, arg.ProjectID, siteModel.CellSize, out var lockResult);

      if (design == null)
      {
        Log.LogWarning($"Failed to read file for design {arg.ReferenceDesign.DesignID}");
        calcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
        return null;
      }

      try
      {
        var result = design.GetBoundary();

        calcResult = result != null ? DesignProfilerRequestResult.OK : DesignProfilerRequestResult.FailedToCalculateBoundary;

        return result;
      }
      finally
      {
        Designs.UnLock(arg.ReferenceDesign.DesignID, design);
      }
    }

    /// <summary>
    /// Performs execution business logic for this executor.
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="calcResult"></param>
    /// <returns></returns>
    public List<Fence> Execute(DesignBoundaryArgument arg, out DesignProfilerRequestResult calcResult)
    {
      // Perform the design boundary calculation
      var result = Calc(arg, out calcResult);

      if (result == null)
      {
        Log.LogInformation($"Unable to calculate a design boundary result for {arg}");
        result = new List<Fence>();
      }

      return result;
    }
  }
}

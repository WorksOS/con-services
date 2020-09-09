using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateDesignElevationSpot
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<CalculateDesignElevationSpot>();

    private readonly IDesignFiles _designs = DIContext.ObtainRequired<IDesignFiles>();

    /// <summary>
    /// Default no-args constructor
    /// </summary>
    public CalculateDesignElevationSpot()
    {
    }

    /// <summary>
    /// Performs the donkey work of the elevation patch calculation
    /// </summary>
    /// <returns>The computed elevation of the given design at the spot location, or NullDouble if the location does not lie on the design</returns>
    private double Calc(ISiteModelBase siteModel, DesignOffset referenceDesign, double spotX, double spotY,
      out DesignProfilerRequestResult calcResult)
    {
      calcResult = DesignProfilerRequestResult.UnknownError;

      var design = _designs.Lock(referenceDesign.DesignID, siteModel, SubGridTreeConsts.DefaultCellSize, out var lockResult);

      if (design == null)
      {
        _log.LogWarning($"Failed to read design file for design {referenceDesign.DesignID}");
        calcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
        return Common.Consts.NullDouble;
      }

      try
      {
        var hint = -1;
        if (design.InterpolateHeight(ref hint, spotX, spotY, referenceDesign.Offset, out var z))
        {
          calcResult = DesignProfilerRequestResult.OK;
        }
        else
        {
          calcResult = DesignProfilerRequestResult.NoElevationsInRequestedPatch;
          z = Common.Consts.NullDouble;
        }

        return z;
      }
      finally
      {
        _designs.UnLock(referenceDesign.DesignID, design);
      }
    }

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    public CalculateDesignElevationSpotResponse Execute(ISiteModelBase siteModel, DesignOffset referenceDesign, double spotX, double spotY)
    {
      try
      {
        var elevation = Calc(siteModel, referenceDesign, spotX, spotY, out var calcResult);

        // Calculate the spot elevation and return it
        return new CalculateDesignElevationSpotResponse
        {
          Elevation = elevation,
          CalcResult = calcResult
        };
      }
      catch (Exception e)
      {
        _log.LogError(e, "Execute: Exception: ");
        return new CalculateDesignElevationSpotResponse
        {
          Elevation = Common.Consts.NullDouble
        };
      }
    }
  }
}

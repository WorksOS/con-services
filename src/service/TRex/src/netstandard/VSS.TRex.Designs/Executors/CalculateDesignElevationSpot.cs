using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateDesignElevationSpot
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignElevationSpot>();

    private readonly IDesignFiles designs = DIContext.Obtain<IDesignFiles>();

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
    private double Calc(Guid projectUID, DesignOffset referenceDesign, double spotX, double spotY,
      out DesignProfilerRequestResult calcResult)
    {
      calcResult = DesignProfilerRequestResult.UnknownError;

      var design = designs.Lock(referenceDesign.DesignID, projectUID, SubGridTreeConsts.DefaultCellSize, out var LockResult);

      if (design == null)
      {
        Log.LogWarning($"Failed to read design file for design {referenceDesign.DesignID}");
        calcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
        return Common.Consts.NullDouble;
      }

      try
      {
        var Hint = -1;
        if (design.InterpolateHeight(ref Hint, spotX, spotY, referenceDesign.Offset, out var Z))
        {
          calcResult = DesignProfilerRequestResult.OK;
        }
        else
        {
          calcResult = DesignProfilerRequestResult.NoElevationsInRequestedPatch;
          Z = Common.Consts.NullDouble;
        }

        return Z;
      }
      finally
      {
        designs.UnLock(referenceDesign.DesignID, design);
      }
    }

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    /// <returns></returns>
    public CalculateDesignElevationSpotResponse Execute(Guid projectUID, DesignOffset referenceDesign, double spotX, double spotY)
    {
      try
      {
        var elevation = Calc(projectUID, referenceDesign, spotX, spotY, out var calcResult);

        // Calculate the spot elevation and return it
        return new CalculateDesignElevationSpotResponse
        {
          Elevation = elevation,
          CalcResult = calcResult
        };
      }
      catch (Exception E)
      {
        Log.LogError(E, "Execute: Exception: ");
        return new CalculateDesignElevationSpotResponse
        {
          Elevation = Common.Consts.NullDouble
        };
      }
    }
  }
}

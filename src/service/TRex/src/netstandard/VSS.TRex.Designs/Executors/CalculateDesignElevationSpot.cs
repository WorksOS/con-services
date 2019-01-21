using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateDesignElevationSpot
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignElevationSpot>();

    private static IDesignFiles designs = null;

    private IDesignFiles Designs => designs ?? (designs = DIContext.Obtain<IDesignFiles>());

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
    private double Calc(Guid projectUID, Guid referenceDesignUID, double spotX, double spotY, double offset,
      out DesignProfilerRequestResult CalcResult)
    {
      CalcResult = DesignProfilerRequestResult.UnknownError;

      IDesignBase Design = Designs.Lock(referenceDesignUID, projectUID, SubGridTreeConsts.DefaultCellSize, out DesignLoadResult LockResult);

      if (Design == null)
      {
        Log.LogWarning($"Failed to read design file for design {referenceDesignUID}");
        CalcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
        return Common.Consts.NullDouble;
      }

      try
      {
        int Hint = -1;
        if (Design.InterpolateHeight(ref Hint, spotX, spotY, offset, out double Z))
        {
          CalcResult = DesignProfilerRequestResult.OK;
        }
        else
        {
          CalcResult = DesignProfilerRequestResult.NoElevationsInRequestedPatch;
          Z = Common.Consts.NullDouble;
        }

        return Z;
      }
      finally
      {
        Designs.UnLock(referenceDesignUID, Design);
      }
    }

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    /// <returns></returns>
    public double Execute(Guid projectUID, Guid referenceDesignUID, double spotX, double spotY, double offset)
    {
      try
      {
        // Calculate the spot elevation and return it
        return Calc(projectUID, referenceDesignUID, spotX, spotY, offset, out DesignProfilerRequestResult CalcResult);
      }
      catch (Exception E)
      {
        Log.LogError(E, "Execute: Exception: ");
        return Common.Consts.NullDouble;
      }
    }
  }
}

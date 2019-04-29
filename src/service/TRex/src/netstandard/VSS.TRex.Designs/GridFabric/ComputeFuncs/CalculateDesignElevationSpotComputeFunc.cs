using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  /// <summary>
  /// Ignite ComputeFunc responsible for executing the elevation patch calculator
  /// </summary>
  public class CalculateDesignElevationSpotComputeFunc : BaseComputeFunc, IComputeFunc<CalculateDesignElevationSpotArgument, double>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignElevationSpotComputeFunc>();

    public double Invoke(CalculateDesignElevationSpotArgument args)
    {
      try
      {
        CalculateDesignElevationSpot Executor = new CalculateDesignElevationSpot();

        return Executor.Execute(args.ProjectID, args.ReferenceDesignUID, args.SpotX, args.SpotY, args.ReferenceOffset);
      }
      catch (Exception E)
      {
        Log.LogError(E, "Exception:");
        return Common.Consts.NullDouble;
      }
    }
  }
}

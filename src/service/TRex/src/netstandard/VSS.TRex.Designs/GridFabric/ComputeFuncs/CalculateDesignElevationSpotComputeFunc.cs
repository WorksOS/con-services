using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  /// <summary>
  /// Ignite ComputeFunc responsible for executing the elevation patch calculator
  /// </summary>
  public class CalculateDesignElevationSpotComputeFunc : BaseComputeFunc, IComputeFunc<CalculateDesignElevationSpotArgument, CalculateDesignElevationSpotResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignElevationSpotComputeFunc>();

    public CalculateDesignElevationSpotResponse Invoke(CalculateDesignElevationSpotArgument args)
    {
      try
      {
        var Executor = new CalculateDesignElevationSpot();

        return Executor.Execute(args.ProjectID, args.ReferenceDesign, args.SpotX, args.SpotY);
      }
      catch (Exception E)
      {
        Log.LogError(E, "Exception:");
        return new CalculateDesignElevationSpotResponse { Elevation = Common.Consts.NullDouble};
      }
    }
  }
}

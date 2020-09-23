using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  /// <summary>
  /// Ignite ComputeFunc responsible for executing the elevation patch calculator
  /// </summary>
  public class CalculateDesignElevationSpotComputeFunc : BaseComputeFunc, IComputeFunc<CalculateDesignElevationSpotArgument, CalculateDesignElevationSpotResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<CalculateDesignElevationSpotComputeFunc>();

    public CalculateDesignElevationSpotResponse Invoke(CalculateDesignElevationSpotArgument args)
    {
      try
      {
        var Executor = new CalculateDesignElevationSpot();

        return Executor.Execute(DIContext.ObtainRequired<ISiteModels>().GetSiteModel(args.ProjectID), args.ReferenceDesign, args.SpotX, args.SpotY);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception calculating design spot height");
        return new CalculateDesignElevationSpotResponse { Elevation = Common.Consts.NullDouble, CalcResult = DesignProfilerRequestResult.UnknownError};
      }
    }
  }
}

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
  /// Ignite ComputeFunc responsible for executing the design profile calculator
  /// </summary>
  public class CalculateDesignProfileComputeFunc : BaseComputeFunc, IComputeFunc<CalculateDesignProfileArgument, CalculateDesignProfileResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<CalculateDesignProfileComputeFunc>();

    public CalculateDesignProfileResponse Invoke(CalculateDesignProfileArgument arg)
    {
      var startDate = DateTime.UtcNow;
      try
      {
        _log.LogInformation($"In: {nameof(CalculateDesignProfileComputeFunc)}: Arg = {arg}");

        var Executor = new CalculateDesignProfile();
        var profile = Executor.Execute(arg, out var calcResult);

        var result = new CalculateDesignProfileResponse
        {
          Profile = profile,
          RequestResult = calcResult
        };

        _log.LogInformation($"Profile result: {result.Profile?.Count ?? -1} vertices");
        return result;
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception calculating design profile");
        return new CalculateDesignProfileResponse { RequestResult = Models.DesignProfilerRequestResult.UnknownError};
      }
      finally
      {
        _log.LogInformation($"Out: CalculateDesignProfileComputeFunc in {DateTime.UtcNow - startDate}, Arg = {arg}");
      }
    }
  }
}

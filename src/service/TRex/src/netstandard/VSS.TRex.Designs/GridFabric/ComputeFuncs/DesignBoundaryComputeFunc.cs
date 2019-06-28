using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  /// <summary>
  /// Design boundary specific request to make to the compute context.
  /// </summary>
  public class DesignBoundaryComputeFunc : BaseComputeFunc, IComputeFunc<DesignBoundaryArgument, DesignBoundaryResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<DesignBoundaryComputeFunc>();

    public DesignBoundaryResponse Invoke(DesignBoundaryArgument arg)
    {
      try
      {
        var executor = new CalculateDesignBoundary();

        var fences = executor.Execute(arg, out var calcResult);

        if (fences == null || fences.Count == 0)
          return new DesignBoundaryResponse
          {
            RequestResult = calcResult
          };

        return new DesignBoundaryResponse
        {
          RequestResult = calcResult,
          Boundary = fences
        };
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Failed to compute design boundary. Site Model ID: {arg.ProjectID} design ID: {arg.ReferenceDesign.DesignID} error: {e.Message}");
        return null;
      }
    }
  }
}

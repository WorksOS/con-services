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

        var fences = executor.Execute(arg.ProjectID, arg.ReferenceDesign.DesignID, arg.Tolerance);

        if (fences == null || fences.Count == 0)
          return new DesignBoundaryResponse
          {
            RequestResult = DesignProfilerRequestResult.FailedToComputeAlignmentVertices
          };

        return new DesignBoundaryResponse
        {
          RequestResult = DesignProfilerRequestResult.OK,
          Boundary = fences
        };
      }
      catch (Exception E)
      {
        Log.LogError(E, "Exception: ");
        return null;
      }
    }
  }
}

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
  public class AlignmentDesignFilterBoundaryComputeFunc : BaseComputeFunc, IComputeFunc<AlignmentDesignFilterBoundaryArgument, AlignmentDesignFilterBoundaryResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignElevationPatchComputeFunc>();

    public AlignmentDesignFilterBoundaryResponse Invoke(AlignmentDesignFilterBoundaryArgument args)
    {
      try
      {
        // Calculate an elevation patch for the requested location and convert it into a bitmask detailing which cells have non-null values

        var Executor = new CalculateAlignmentDesignFilterBoundary();

        var fence = Executor.Execute(args.ProjectID, args.ReferenceDesignUID, args.StartStation, args.EndStation, args.LeftOffset, args.RightOffset);

        if (fence == null || !fence.HasVertices)
          return new AlignmentDesignFilterBoundaryResponse
          {
            RequestResult = DesignProfilerRequestResult.FailedToComputeAlignmentVertices
          };

        return new AlignmentDesignFilterBoundaryResponse
        {
          RequestResult = DesignProfilerRequestResult.OK,
          Boundary = fence
        };
      }
      catch (Exception E)
      {
        Log.LogError("Exception: ", E);
        return null;
      }
    }
  }
}

using System;
using System.Threading.Tasks;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  public class AlignmentDesignFilterBoundaryRequest : DesignProfilerRequest<AlignmentDesignFilterBoundaryArgument, AlignmentDesignFilterBoundaryResponse>
  {
    public override AlignmentDesignFilterBoundaryResponse Execute(AlignmentDesignFilterBoundaryArgument arg)
    {
      // Construct the function to be used
      var func = new AlignmentDesignFilterBoundaryComputeFunc();

      return Compute.Apply(func, arg);
    }

    public override Task<AlignmentDesignFilterBoundaryResponse> ExecuteAsync(AlignmentDesignFilterBoundaryArgument arg)
    {
      // Construct the function to be used
      var func = new AlignmentDesignFilterBoundaryComputeFunc();

      return Compute.ApplyAsync(func, arg);
    }

    public static AlignmentDesignFilterBoundaryResponse Execute(DesignOffset referenceDesign, double startStation, double endStation, double leftOffset, double rightOffset)
    {
      var request = new AlignmentDesignFilterBoundaryRequest();

      return request.Execute(new AlignmentDesignFilterBoundaryArgument
      {
        ReferenceDesign = referenceDesign,
        StartStation = startStation,
        EndStation = endStation,
        LeftOffset = leftOffset,
        RightOffset = rightOffset
      });
    }

    public static Task<AlignmentDesignFilterBoundaryResponse> ExecuteAsync(DesignOffset referenceDesign, double startStation, double endStation, double leftOffset, double rightOffset)
    {
      var request = new AlignmentDesignFilterBoundaryRequest();

      return request.ExecuteAsync(new AlignmentDesignFilterBoundaryArgument
      {
        ReferenceDesign = referenceDesign,
        StartStation = startStation,
        EndStation = endStation,
        LeftOffset = leftOffset,
        RightOffset = rightOffset
      });
    }
  }
}

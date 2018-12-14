using System;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  public class AlignmentDesignFilterBoundaryRequest : DesignProfilerRequest<AlignmentDesignFilterBoundaryArgument, AlignmentDesignFilterBoundaryResponse>
  {
    public override AlignmentDesignFilterBoundaryResponse Execute(AlignmentDesignFilterBoundaryArgument arg)
    {
      // Construct the function to be used
      var func = new AlignmentDesignFilterBoundaryComputeFunc();

      return _Compute.Apply(func, arg);
    }

    public static AlignmentDesignFilterBoundaryResponse Execute(Guid referenceDesignUID, double startStation, double endStation, double leftOffset, double rightOffset)
    {
      var request = new AlignmentDesignFilterBoundaryRequest();

      return request.Execute(new AlignmentDesignFilterBoundaryArgument
      {
        ReferenceDesignUID = referenceDesignUID,
        StartStation = startStation,
        EndStation = endStation,
        LeftOffset = leftOffset,
        RightOffset = rightOffset
      });
    }
  }
}

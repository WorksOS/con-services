using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  /// <summary>
  /// Provides a request that queries a design surface across the cells in a sub grid to determine which
  /// cells have non-null elevations and returns a bitmask detailing the result
  /// </summary>
  public class DesignFilterSubGridMaskRequest : DesignProfilerRequest<DesignSubGridFilterMaskArgument, DesignFilterSubGridMaskResponse>
  {
    public override DesignFilterSubGridMaskResponse Execute(DesignSubGridFilterMaskArgument arg)
    {
     var func = new DesignFilterSubGridMaskComputeFunc();

      return Compute.Apply(func, arg);
    }
  }
}

using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  /// <summary>
  /// Provides a request that queries a design surface across the cells in a subgrid to determine which
  /// cells have non-null elevations and returns a bitmask detailing the result
  /// </summary>
  public class DesignFilterSubGridMaskRequest : DesignProfilerRequest<DesignSubGridFilterMaskArgument, DesignFilterSubGridMaskResponse>
  {
    public override DesignFilterSubGridMaskResponse Execute(DesignSubGridFilterMaskArgument arg)
    {
      // Construct the function to be used
      /*IComputeFunc<CalculateDesignElevationPatchArgument, SubGridTreeLeafBitmapSubGrid> */

      var func = new DesignFilterSubGridMaskComputeFunc();

      return _Compute.Apply(func, arg);
    }
  }
}

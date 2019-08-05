using System.Threading.Tasks;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  public class DesignElevationPatchRequest : DesignProfilerRequest<CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>
  {
    public override CalculateDesignElevationPatchResponse Execute(CalculateDesignElevationPatchArgument arg)
    {
      // Construct the function to be used
      var func = new CalculateDesignElevationPatchComputeFunc();

      return Compute.Apply(func, arg);
    }

    public override Task<CalculateDesignElevationPatchResponse> ExecuteAsync(CalculateDesignElevationPatchArgument arg)
    {
      // Construct the function to be used
      var func = new CalculateDesignElevationPatchComputeFunc();

      return Compute.ApplyAsync(func, arg);
    }
  }
}

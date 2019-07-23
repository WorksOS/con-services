using System.Threading.Tasks;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  public class DesignProfileRequest : DesignProfilerRequest<CalculateDesignProfileArgument, CalculateDesignProfileResponse>
  {
    public override CalculateDesignProfileResponse Execute(CalculateDesignProfileArgument arg)
    {
      // Construct the function to be used
      var func = new CalculateDesignProfileComputeFunc();

      // Send the appropriate response to the caller
      return Compute.Apply(func, arg);
    }

    public override Task<CalculateDesignProfileResponse> ExecuteAsync(CalculateDesignProfileArgument arg)
    {
      // Construct the function to be used
      var func = new CalculateDesignProfileComputeFunc();

      // Send the appropriate response to the caller
      return Compute.ApplyAsync(func, arg);
    }
  }
}

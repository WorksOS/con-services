using Apache.Ignite.Core.Compute;
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
    IComputeFunc< CalculateDesignProfileArgument, CalculateDesignProfileResponse> func = new CalculateDesignProfileComputeFunc();

    // Send the appropriate response to the caller
    return _Compute.Apply(func, arg);

      //  Task<CalculateDesignProfileResponse> taskResult = compute.ApplyAsync(func, arg);

      // Send the appropriate response to the caller
      //            return taskResult.Result;
    }
  }
}

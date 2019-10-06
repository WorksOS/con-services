using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.GridFabric.Requests
{
  public class OverrideEventRequest : TAGFileProcessingPoolRequest<OverrideEventRequestArgument, OverrideEventResponse>
  {
    /// <summary>
    /// Local reference to the compute func used to execute the override event request on the grid.
    /// </summary>
    private IComputeFunc<OverrideEventRequestArgument, OverrideEventResponse> func;

    /// <summary>
    /// No-arg constructor that creates a default override event request with a singleton ComputeFunc
    /// </summary>
    public OverrideEventRequest()
    {
      // Construct the function to be used
      func = new OverrideEventComputeFunc();
    }

    /// <summary>
    /// Processes an override event for a machine into a project synchronously
    /// </summary>
    public override OverrideEventResponse Execute(OverrideEventRequestArgument arg) => Compute.Apply(func, arg);

    /// <summary>
    /// Processes an override event for  a machine into a project asynchronously
    /// </summary>
    public override Task<OverrideEventResponse> ExecuteAsync(OverrideEventRequestArgument arg) => Compute.ApplyAsync(func, arg);
  }
}

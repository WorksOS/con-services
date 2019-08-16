using Apache.Ignite.Core.Compute;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.GridFabric.ComputeFuncs
{
  public class OverrideEventComputeFunc : BaseComputeFunc, IComputeFunc<OverrideEventRequestArgument, OverrideEventResponse>
  {
    /// <summary>
    /// Default no-arg constructor that orients the request to the available TAG processing server nodes on the mutable grid projection
    /// </summary>
    public OverrideEventComputeFunc()
    {
    }

    /// <summary>
    /// The Invoke method for the compute func - calls the override event executor to do the work
    /// </summary>
    public OverrideEventResponse Invoke(OverrideEventRequestArgument arg)
    {
      var executor = new OverrideEventExecutor();
      return executor.ExecuteAsync(arg).WaitAndUnwrapException();
    }
  }
}

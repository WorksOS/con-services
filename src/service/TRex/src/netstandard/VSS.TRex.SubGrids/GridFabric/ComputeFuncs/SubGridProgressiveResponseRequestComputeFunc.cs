using System.Diagnostics;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.SubGrids.Interfaces;

namespace VSS.TRex.SubGrids.GridFabric.ComputeFuncs
{
  public class SubGridProgressiveResponseRequestComputeFunc : BaseComputeFunc, IComputeFunc<ISubGridProgressiveResponseRequestComputeFuncArgument, bool>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridProgressiveResponseRequestComputeFunc>();

    /// <summary>
    /// Relays the partial sub grids request response to the orchestrating request
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public bool Invoke(ISubGridProgressiveResponseRequestComputeFuncArgument arg)
    {
      var listener = DIContext.Obtain<IPipelineListenerMapper>().Find(arg.RequestDescriptor);

      if (listener == null)
      {
        Log.LogError($"Listener for request {arg.RequestDescriptor} is null");
      }

      var sw = Stopwatch.StartNew();
      var result = listener?.Invoke(arg.NodeId, arg.Payload) ?? false;
      Log.LogDebug($"SubGridProgressiveResponseRequestComputeFunc.Invoke({arg.NodeId}), size {arg.Payload.Bytes.Length} result {result} completed in {sw.Elapsed}");

      return result;
    }
  }
}

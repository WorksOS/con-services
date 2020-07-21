using System;
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
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SubGridProgressiveResponseRequestComputeFunc>();

    /// <summary>
    /// Relays the partial sub grids request response to the orchestrating request
    /// </summary>
    public bool Invoke(ISubGridProgressiveResponseRequestComputeFuncArgument arg)
    {
      try
      {
        var listener = DIContext.Obtain<IPipelineListenerMapper>().Find(arg.RequestDescriptor);

        if (listener == null)
        {
          _log.LogError($"Listener for request {arg.RequestDescriptor} is null");
        }

        var sw = Stopwatch.StartNew();
        var result = listener?.Invoke(arg.NodeId, arg.Payload) ?? false;
        _log.LogDebug($"SubGridProgressiveResponseRequestComputeFunc.Invoke({arg.NodeId}), size {arg.Payload.Bytes.Length} result {result} completed in {sw.Elapsed}");

        return result;
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Unhandled exception in {nameof(Invoke)}");
        return false;
      }
    }
  }
}

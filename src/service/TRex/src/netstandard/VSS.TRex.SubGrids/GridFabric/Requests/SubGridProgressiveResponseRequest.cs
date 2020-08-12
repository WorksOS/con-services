using System;
using System.Collections.Generic;
using System.Diagnostics;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Interfaces;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VSS.TRex.SubGrids.GridFabric.Requests
{
  /// <summary>
  /// Sends a compute request to the originating node of a sub grids request containing a partial
  /// result of the set of sub grids being requested. By definition the originator node is a part
  /// of the immutable grid, and is identified by the Ignite node ID (provided as the TRexNodeID
  /// parameter of the sub grids request.
  /// This is a very specific style of request with a particular target that does not fit the BaseIgniteClass
  /// model of requests in TRex as the source node may be of a number of roles, and a specific node
  /// rather than an arbitrary node of a role is required to receive the request
  /// The payload is a generic byte array.
  /// </summary>
  public class SubGridProgressiveResponseRequest : BaseIgniteClass, ISubGridProgressiveResponseRequest
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SubGridProgressiveResponseRequest>();

    /// <summary>
    /// The compute projection representing the Ignite node identified by _nodeId
    /// </summary>
    private readonly ICompute _immutableCompute;

    /// <summary>
    /// Immutable grid reference only
    /// </summary>
    private IIgnite _immutableIgnite;
    private IIgnite ImmutableIgnite => _immutableIgnite ??= DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable);

    /// <summary>
    /// Default no-arg constructor - used by unit tests
    /// </summary>
    public SubGridProgressiveResponseRequest() { }

    private readonly SubGridProgressiveResponseRequestComputeFunc _computeFunc = new SubGridProgressiveResponseRequestComputeFunc();

    /// <summary>
    /// Executes the request to send the payload to the originating node
    /// </summary>
    public bool Execute(ISubGridProgressiveResponseRequestComputeFuncArgument arg)
    {
      try
      {
        var sw = Stopwatch.StartNew();

        var result = _immutableCompute.Apply(_computeFunc, arg);

        _log.LogDebug($"SubGridProgressiveResponseRequest.Execute() for request {arg.RequestDescriptor} with {arg.Payload.Bytes.Length} bytes completed in {sw.Elapsed}");
        return result;
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Exception in {nameof(SubGridProgressiveResponseRequest)}");
        return false;
      }
    }

    /// <summary>
    /// Construct a progressive response request that will execute progressive request responses to the
    /// Ignite node identified by nodeId
    /// </summary>
    /// <param name="nodeId">The internal Ignite identifier of the node origination the request for the sub grids being returned</param>
    public SubGridProgressiveResponseRequest(Guid nodeId)
    {
      // Determine the single node the request needs to be sent to
      _immutableCompute = ImmutableIgnite
        .GetCluster()
        .ForNodeIds(new List<Guid> {nodeId})
        .GetCompute()
        .WithExecutor(TREX_PROGRESSIVE_QUERY_CUSTOM_THREAD_POOL_NAME);

      if (_immutableCompute == null)
      {
        _log.LogWarning($"Failed to creating SubGridProgressiveResponseRequest instance for node '{nodeId}'. Compute cluster projection is null");
      }
      else
      {
        var projectionSize = _immutableCompute?.ClusterGroup?.GetNodes()?.Count ?? 0;
        if (projectionSize == 0)
        {
          _log.LogWarning($"Failed to creating SubGridProgressiveResponseRequest instance for node '{nodeId}'. Topology projection contains no nodes");
        }
      }
    }
  }
}

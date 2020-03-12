using System;
using System.Collections.Generic;
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
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridProgressiveResponseRequest>();

    /// <summary>
    /// The compute projection representing the Ignite node identified by _nodeId
    /// </summary>
    private readonly ICompute _compute;

    /// <summary>
    /// Default no-arg constructor - used by unit tests
    /// </summary>
    public SubGridProgressiveResponseRequest() { }

    private readonly SubGridProgressiveResponseRequestComputeFunc _computeFunc = new SubGridProgressiveResponseRequestComputeFunc();

    /// <summary>
    /// Executes the request to send the payload to the originating node
    /// </summary>
    /// <param name="arg"></param>
    public bool Execute(ISubGridProgressiveResponseRequestComputeFuncArgument arg)
    {
      try
      {
        return _compute.Apply(_computeFunc, arg);
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception in {nameof(SubGridProgressiveResponseRequest)}");
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
      // Immutable grid reference only
      var ignite = DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable);

      // Determine the single node the request needs to be sent to
      _compute = ignite
        .GetCluster()
        .ForNodeIds(new List<Guid> {nodeId})
        .GetCompute()
        .WithExecutor(BaseIgniteClass.TRexProgressiveQueryCustomThreadPoolName);

      // Log.LogDebug($"Creating SubGridProgressiveResponseRequest instance for node {nodeId} against compute cluster with {_compute.ClusterGroup.GetNodes().Count} nodes");
    }
  }
}

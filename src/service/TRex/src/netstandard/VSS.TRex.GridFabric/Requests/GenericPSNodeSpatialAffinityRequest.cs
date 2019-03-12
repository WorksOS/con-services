using Apache.Ignite.Core.Compute;
using System.Threading.Tasks;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Caches;

namespace VSS.TRex.GridFabric.Requests
{
  /// <summary>
  /// Provides a generic class for making requests to a single node in the 'PSNode' compute cluster.
  /// This class coordinates the affinity relationship of the request to the compute cluster and returns
  /// the final result
  /// </summary>
  /// <typeparam name="TArgument"></typeparam>
  /// <typeparam name="TComputeFunc"></typeparam>
  /// <typeparam name="TResponse"></typeparam>
  public abstract class GenericPSNodeSpatialAffinityRequest<TArgument, TComputeFunc, TResponse> : CacheComputePoolRequest<TArgument, TResponse>
    where TComputeFunc : IComputeFunc<TResponse>, IComputeFuncArgument<TArgument>, new()
    where TResponse : class
  {
    /// <summary>
    /// Executes a request through it's generic types
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="key">The spatial affinity key to be used to direct this request to the node owning the partition it maps to</param>
    /// <returns></returns>
    public virtual TResponse Execute(TArgument arg, ISubGridSpatialAffinityKey key)
    {
      if (key == null)
        throw new TRexException("Affinity based result execution requires an affinity key");

      // Construct the function to be used
      var func = new TComputeFunc
      {
        Argument = arg
      };

      // Send the result to the affinity bound node compute pool
      return Compute?.AffinityCall(TRexCaches.ImmutableSpatialCacheName(), key, func);
    }

    /// <summary>
    /// Executes a request through it's generic types asynchronously
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="key">The spatial affinity key to be used to direct this request to the node owning the partition it maps to</param>
    /// <returns></returns>
    public virtual Task<TResponse> ExecuteAsync(TArgument arg, ISubGridSpatialAffinityKey key)
    {
      if (key == null)
        throw new TRexException("Affinity based result execution requires an affinity key");

      // Construct the function to be used
      var func = new TComputeFunc
      {
        Argument = arg
      };

      // Send the result to the affinity bound node compute pool
      return Compute?.AffinityCallAsync(TRexCaches.ImmutableSpatialCacheName(), key, func);
    }

    public override TResponse Execute(TArgument arg) => throw new TRexException("Affinity based result execution requires an affinity key");
    public override Task<TResponse> ExecuteAsync(TArgument arg) => throw new TRexException("Affinity based result execution requires an affinity key");
  }
}

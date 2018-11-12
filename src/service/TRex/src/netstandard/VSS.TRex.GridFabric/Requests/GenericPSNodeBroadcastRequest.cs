using Apache.Ignite.Core.Compute;
using System.Collections.Generic;
using System.Linq;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Requests
{
    /// <summary>
    /// Provides a generic class for making mapReduce style requests to the 'PSNode' compute cluster.
    /// This class coordinates broadcasting of requests to the compute cluster and reduce the final result
    /// from the per-partition results returned to it.
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TComputeFunc"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public abstract class GenericPSNodeBroadcastRequest<TArgument, TComputeFunc, TResponse> : CacheComputePoolRequest<TArgument, TResponse>
        where TComputeFunc : IComputeFunc<TArgument, TResponse>, new()
        where TResponse : class, IAggregateWith<TResponse>
    {
        /// <summary>
        /// Executes a request through it's generic types
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public override TResponse Execute(TArgument arg)
        {
            // Construct the function to be used
            TComputeFunc func = new TComputeFunc();

            // Broadcast the request to the compute pool and assemble a list of the results
            //            Task<ICollection<TResponse>> taskResult = _Compute?.BroadcastAsync(func, arg);
            ICollection<TResponse> Result = _Compute?.Broadcast(func, arg);

            // Reduce the set of results to a single aggregated result and send the result back
            // If there is no task result then return a null response
            //return taskResult?.Result?.Count > 0 ? taskResult.Result.Aggregate((first, second) => first.AggregateWith(second))
            return Result?.Count > 0 ? Result.Aggregate((first, second) => first.AggregateWith(second)) : null;
        }
    }
}

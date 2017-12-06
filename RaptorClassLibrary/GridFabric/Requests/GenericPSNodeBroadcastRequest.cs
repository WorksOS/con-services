using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    /// <summary>
    /// Provides a highly genericised class for making mapReduce style requests to the 'PSNode' compute cluster
    /// </summary>
    /// <typeparam name="Argument"></typeparam>
    /// <typeparam name="ComputeFunc"></typeparam>
    /// <typeparam name="Response"></typeparam>
    public class GenericPSNodeBroadcastRequest<Argument, ComputeFunc, Response> : ApplicationServicePoolRequest
        where ComputeFunc : IComputeFunc<Argument, Response>, new()
        where Response : IResponseAggregateWith<Response>
    {
        /// <summary>
        /// Executes a request genericised through it's templated types
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public Response Execute(Argument arg)
        {
            // Construct the function to be used
            IComputeFunc<Argument, Response> func = new ComputeFunc();

            // Broadcast the request to the compute pool and assembly a list of the results
            Task<ICollection<Response>> taskResult = _Compute.BroadcastAsync(func, arg);

            // Reduce the set of results to a single volumes result and send the result back
            return taskResult.Result.Aggregate((first, second) => first.AggregateWith(second));
        }
    }
}
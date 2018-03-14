using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    /// <summary>
    /// Provides a highly genericised class for making a request to a member of the 'ASNode' node pool
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TComputeFunc"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class GenericASNodeRequest<TArgument, TComputeFunc, TResponse> : ApplicationServicePoolRequest
        where TComputeFunc : IComputeFunc<TArgument, TResponse>, new()
        where TResponse : class, new()
    {
        /// <summary>
        /// Renders a bitmap according to the parameters in its argument
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public virtual TResponse Execute(TArgument arg)
        {
            // Construct the function to be used
            IComputeFunc<TArgument, TResponse> func = new TComputeFunc();

            // Send the request to the application service pool and retrieve the resul
            Task<TResponse> taskResult = _Compute.ApplyAsync(func, arg);

            // Send the response to the caller
            // If there is no task result then return an empty response
            return (taskResult?.Result != null) ? taskResult.Result : new TResponse();
        }
    }
}

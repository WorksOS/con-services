using Apache.Ignite.Core.Compute;
using System;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Requests
{
    /// <summary>
    /// Provides a highly genericised class for making a request to a member of the 'ASNode' node pool
    /// </summary>
    /// <typeparam name="TArgument"></typeparam>
    /// <typeparam name="TComputeFunc"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    [Serializable]
    public class GenericASNodeRequest<TArgument, TComputeFunc, TResponse> : ApplicationServicePoolRequest<TArgument, TResponse>, IGenericASNodeRequest<TArgument, TResponse>
        where TComputeFunc : IComputeFunc<TArgument, TResponse>, new()
        where TResponse : class, new()
    {
      public GenericASNodeRequest(string gridName, string role) : base(gridName, role)
      {
      }

    /// <summary>
    /// Executes the generic request by instantiating the required ComputeFunc and sending it to 
    /// the compute projection on the grid as defined by the GridName and Role parameters in this request
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public override TResponse Execute(TArgument arg)
        {
            // Construct the function to be used
            TComputeFunc func = new TComputeFunc();

            // Send the request to the application service pool and retrieve the resul
            //Task<TResponse> taskResult = _Compute.ApplyAsync(func, arg);
            TResponse Result = _Compute.Apply(func, arg);

            // Send the response to the caller
            // If there is no task result then return a null response
            //return taskResult?.Result != null) ? taskResult.Result : null;
            return Result;
        }
    }
}

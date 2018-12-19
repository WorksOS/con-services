using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Requests
{
  /// <summary>
  /// Provides a highly generic class for making a request to a member of the 'ASNode' node pool
  /// </summary>
  /// <typeparam name="TArgument"></typeparam>
  /// <typeparam name="TComputeFunc"></typeparam>
  /// <typeparam name="TResponse"></typeparam>
  public abstract class GenericASNodeRequest<TArgument, TComputeFunc, TResponse> : ApplicationServicePoolRequest<TArgument, TResponse>, IGenericASNodeRequest<TArgument, TResponse>
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

      // Send the request to the application service pool and retrieve the result
      // and return it to the caller
      return _Compute.Apply(func, arg);
    }

    /// <summary>
    /// Executes the generic request by instantiating the required ComputeFunc and sending it to 
    /// the compute projection on the grid as defined by the GridName and Role parameters in this request
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public override Task<TResponse> ExecuteAsync(TArgument arg)
    {
      // Construct the function to be used
      TComputeFunc func = new TComputeFunc();

      // Send the request to the application service pool and retrieve the result
      return _Compute.ApplyAsync(func, arg);
    }
  }
}

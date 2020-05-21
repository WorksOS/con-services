using System;
using System.Threading;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Designs.GridFabric.Requests
{
  /// <summary>
  /// Provides a highly generic class for making a request to a member of the 'ASNode' node pool
  /// </summary>
  /// <typeparam name="TArgument"></typeparam>
  /// <typeparam name="TComputeFunc"></typeparam>
  /// <typeparam name="TResponse"></typeparam>
  public abstract class GenericDesignProfilerRequest<TArgument, TComputeFunc, TResponse> : DesignProfilerServicePoolRequest<TArgument, TResponse>, IGenericASNodeRequest<TArgument, TResponse>
    where TComputeFunc : IComputeFunc<TArgument, TResponse>, new()
    where TResponse : class, new()
  {
    public GenericDesignProfilerRequest() : base()
    {
    }

    /// <summary>
    /// Time out any grid functions after this timeout
    /// </summary>
    public virtual TimeSpan Timeout => new TimeSpan(0, 0, 2, 0);

    /// <summary>
    /// Executes the generic request by instantiating the required ComputeFunc and sending it to 
    /// the compute projection on the grid as defined by the GridName and Role parameters in this request
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public override TResponse Execute(TArgument arg)
    {
      // Construct the function to be used
      var func = new TComputeFunc();

      // Send the request to the application service pool and retrieve the result
      // and return it to the caller
      return Compute.Apply(func, arg);
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
      var func = new TComputeFunc();

      var cts = new CancellationTokenSource(Timeout);

      // Send the request to the application service pool and retrieve the result
      return Compute.ApplyAsync(func, arg, cts.Token);
    }
  }
}

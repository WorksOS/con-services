using Apache.Ignite.Core.Compute;
using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.GridFabric.Responses;

namespace VSS.TRex.SubGrids.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The closure/function that implements subgrid request processing on compute nodes
  /// </summary>
  public abstract class SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse> : BaseComputeFunc, IComputeFunc<TSubGridsRequestArgument, TSubGridRequestsResponse>, IDisposable
    where TSubGridsRequestArgument : SubGridsRequestArgument
    where TSubGridRequestsResponse : SubGridRequestsResponse, new()
  {
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    protected SubGridsRequestComputeFuncBase()
    {
    }

    protected abstract SubGridsRequestComputeFuncBase_Executor<TSubGridsRequestArgument, TSubGridRequestsResponse> GetExecutor();

    /// <summary>
    /// Invoke function called in the context of the cluster compute node
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public TSubGridRequestsResponse Invoke(TSubGridsRequestArgument arg)
    {
      TSubGridRequestsResponse result;

      Log.LogInformation("In SubGridsRequestComputeFunc.invoke()");

      try
      {
        try
        {
          var Executor = GetExecutor();

          Executor.InitialiseComputeFunc();
          Executor.UnpackArgument(arg);

          result = Executor.Execute();
        }
        finally
        {
          Log.LogInformation("Out SubGridsRequestComputeFunc.invoke()");
        }
      }
      catch (Exception E)
      {
        Log.LogError($"Exception occurred:\n{E}");

        return new TSubGridRequestsResponse {ResponseCode = SubGridRequestsResponseResult.Unknown};
      }

      return result;
    }

    protected virtual void DoDispose()
    {
      // No dispose behaviour for base compute function
    }

    /// <summary>
    /// Implementation of the IDisposable interface
    /// </summary>
    public void Dispose()
    {
      DoDispose();
    }
  }
}

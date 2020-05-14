using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.Executors;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The grid compute function responsible for coordinating sub grids comprising a patch a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  public class DeleteSiteModelRequestComputeFunc : BaseComputeFunc, IComputeFunc<DeleteSiteModelRequestArgument, DeleteSiteModelRequestResponse>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<DeleteSiteModelRequestComputeFunc>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public DeleteSiteModelRequestComputeFunc()
    {
    }

    public DeleteSiteModelRequestResponse Invoke(DeleteSiteModelRequestArgument arg)
    {
      Log.LogInformation("In GridRequestComputeFunc.Invoke()");

      try
      {
        try
        {
          var request = new DeleteSiteModelComputeFuncExecutor(arg);

          Log.LogInformation("Executing request.ExecuteAsync()");

          if (!request.ExecuteAsync().WaitAndUnwrapException())
            Log.LogError($"Request execution failed");

          return request.Response;
        }
        finally
        {
          Log.LogInformation("Exiting GridRequestComputeFunc.Invoke()");
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Unhandled exception in {nameof(Invoke)}");
        return new DeleteSiteModelRequestResponse {Result = DeleteSiteModelResult.UnhandledException};
      }
    }
  }
}

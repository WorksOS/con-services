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
  /// The grid compute function responsible for coordinating deletion of a project from within the TRex persistent data stores
  /// </summary>
  public class DeleteSiteModelRequestComputeFunc : BaseComputeFunc, IComputeFunc<DeleteSiteModelRequestArgument, DeleteSiteModelRequestResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<DeleteSiteModelRequestComputeFunc>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public DeleteSiteModelRequestComputeFunc()
    {
    }

    public DeleteSiteModelRequestResponse Invoke(DeleteSiteModelRequestArgument arg)
    {
      _log.LogInformation($"In {nameof(DeleteSiteModelRequestComputeFunc)}.Invoke()");

      try
      {
        try
        {
          var request = new DeleteSiteModelComputeFuncExecutor(arg);

          _log.LogInformation("Executing request.ExecuteAsync()");

          if (!request.ExecuteAsync().WaitAndUnwrapException())
            _log.LogError($"Request execution failed: {request?.Response?.Result ?? DeleteSiteModelResult.UnknownError}");

          return request.Response;
        }
        finally
        {
          _log.LogInformation($"Exiting {nameof(DeleteSiteModelRequestComputeFunc)}.Invoke()");
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Unhandled exception in {nameof(Invoke)}");
        return new DeleteSiteModelRequestResponse { Result = DeleteSiteModelResult.UnhandledException };
      }
    }
  }
}

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
  /// The grid compute function responsible for coordinating rebuilding of a project (optionally) based on a pre-existing project and 
  /// a set of TAG files from the TRex processed TAG file archive in S3
  /// </summary>
  public class RebuildSiteModelRequestComputeFunc : BaseComputeFunc, IComputeFunc<RebuildSiteModelRequestArgument, RebuildSiteModelRequestResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<DeleteSiteModelRequestComputeFunc>();

    /// <summary>
    /// Default no-arg constructor that orients the request to the available servers on the immutable grid projection
    /// </summary>
    public RebuildSiteModelRequestComputeFunc()
    {
    }

    public RebuildSiteModelRequestResponse Invoke(RebuildSiteModelRequestArgument arg)
    {
      _log.LogInformation($"In {nameof(RebuildSiteModelRequestComputeFunc)}.Invoke()");

      try
      {
        try
        {
          var request = new RebuildSiteModelComputeFuncExecutor(arg);

          _log.LogInformation("Executing request.ExecuteAsync()");

          if (!request.ExecuteAsync().WaitAndUnwrapException())
            _log.LogError($"Request execution failed: {request?.Response?.RebuildResult ?? RebuildSiteModelResult.UnknownError}");

          return request.Response;
        }
        finally
        {
          _log.LogInformation($"Exiting {nameof(RebuildSiteModelRequestComputeFunc)}.Invoke()");
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Unhandled exception in {nameof(Invoke)}");
        return new RebuildSiteModelRequestResponse(arg.ProjectID) { RebuildResult = RebuildSiteModelResult.UnhandledException };
      }
    }
  }
}

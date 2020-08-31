using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs
{
  /// <summary>
  /// This compute func operates in the context of an application server that reaches out to the compute cluster to 
  /// perform subgrid processing.
  /// </summary>
  public class AnalyticsComputeFunc_ApplicationService<TArgument, TResponse, TRequest> : BaseComputeFunc, IComputeFunc<TArgument, TResponse>
      where TArgument : class
      where TResponse : BaseAnalyticsResponse, IAggregateWith<TResponse>, new()
      where TRequest : BaseRequest<TArgument, TResponse>, new()
  {
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ILogger _log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    public TResponse Invoke(TArgument arg)
    {
      Stopwatch invokeStopWatch = null;
      try
      {
        invokeStopWatch = Stopwatch.StartNew();
        _log.LogInformation("In AnalyticsComputeFunc_ApplicationService.Invoke()");

        var request = new TRequest();

        _log.LogInformation("Executing AnalyticsComputeFunc_ApplicationService.Execute()");

        return request.Execute(arg);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception in generic application service analytics compute func");
        return new TResponse() { ResultStatus = Types.RequestErrorStatus.Exception };
      }
      finally
      {
        _log.LogInformation($"Exiting AnalyticsComputeFunc_ApplicationService.Invoke(), elapsed time = {invokeStopWatch?.Elapsed}");
      }
    }
  }
}

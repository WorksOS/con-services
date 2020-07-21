using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.Analytics.Foundation.Coordinators;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Interfaces;
using System;

namespace VSS.TRex.Analytics.Foundation.GridFabric.ComputeFuncs
{
  public class AnalyticsComputeFunc_ClusterCompute<TArgument, TResponse, TCoordinator> : BaseComputeFunc, IComputeFunc<TArgument, TResponse>
      where TArgument : BaseApplicationServiceRequestArgument
      where TResponse : BaseAnalyticsResponse, IAggregateWith<TResponse>, new()
      where TCoordinator : BaseAnalyticsCoordinator<TArgument, TResponse>, new()
  {
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ILogger _log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    public AnalyticsComputeFunc_ClusterCompute()
    {
    }

    /// <summary>
    /// Invoke the statistics request locally on this node
    /// </summary>
    public TResponse Invoke(TArgument arg)
    {
      _log.LogInformation("In AnalyticsComputeFunc_ClusterCompute.Invoke()");

      try
      {
        _log.LogInformation("Executing AnalyticsComputeFunc_ClusterCompute.ExecuteAsync()");

        var coordinator = new TCoordinator();
        return coordinator.ExecuteAsync(arg).WaitAndUnwrapException();
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception in cluster compute generic analytics compute func");
        return new TResponse() { ResultStatus = Types.RequestErrorStatus.Exception};
      }
      finally
      {
        _log.LogInformation("Exiting AnalyticsComputeFunc_ClusterCompute.Invoke()");
      }
    }
  }
}

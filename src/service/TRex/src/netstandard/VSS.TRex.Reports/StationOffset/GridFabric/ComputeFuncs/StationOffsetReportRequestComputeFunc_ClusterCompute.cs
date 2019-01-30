using System.Reflection;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Reports.StationOffset.Executors;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;

namespace VSS.TRex.Reports.StationOffset.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The logic Ignite executes on cluster compute nodes for stationOffset requests
  /// </summary>
  public class StationOffsetReportRequestComputeFunc_ClusterCompute : BaseComputeFunc, IComputeFunc<StationOffsetReportRequestArgument_ClusterCompute, StationOffsetReportRequestResponse_ClusterCompute> 
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    public StationOffsetReportRequestResponse_ClusterCompute Invoke(StationOffsetReportRequestArgument_ClusterCompute arg)
    {
      Log.LogInformation($"Start {nameof(StationOffsetReportRequestResponse_ClusterCompute)}");
      try
      {
        var executor = new ComputeStationOffsetReportExecutor_ClusterCompute(arg);
        return executor.Execute();
      }
      finally
      {
        Log.LogInformation($"End {nameof(StationOffsetReportRequestResponse_ClusterCompute)}");
      }
    }
  }
}


using System;
using System.Reflection;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Reports.StationOffset.Executors;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using VSS.TRex.Types;

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
      catch (Exception e)
      {
        Log.LogError(e, $"{nameof(StationOffsetReportRequestResponse_ClusterCompute)}: Unexpected exception.");
        return new StationOffsetReportRequestResponse_ClusterCompute(){ResultStatus = RequestErrorStatus.Unknown, ReturnCode = ReportReturnCode.UnknownError};
      }
      finally
      {
        Log.LogInformation($"End {nameof(StationOffsetReportRequestResponse_ClusterCompute)}");
      }
    }
  }
}


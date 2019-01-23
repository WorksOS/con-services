using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Reports.StationOffset.Executors;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;

namespace VSS.TRex.Reports.StationOffset.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The logic Ignite executes on cluster compute nodes for profile requests
  /// </summary>
  public class StationOffsetReportRequestComputeFunc_ClusterCompute : BaseComputeFunc, IComputeFunc<StationOffsetReportRequestArgument_ClusterCompute, StationOffsetReportRequestResponse> 
  {
    private static ILogger Log = Logging.Logger.CreateLogger<StationOffsetReportRequestComputeFunc_ClusterCompute>();

    public StationOffsetReportRequestResponse Invoke(StationOffsetReportRequestArgument_ClusterCompute arg)
    {
      Log.LogInformation("In StationOffsetReportRequestComputeFunc_ClusterCompute.Invoke()");

      try
      {
        var executor = new ComputeStationOffsetReportExecutor_ClusterCompute(arg);

        Log.LogInformation("Executing profiler.Execute()");

        return executor.Execute();
      }
      finally
      {
        Log.LogInformation("Exiting StationOffsetReportRequestComputeFunc_ClusterCompute.Invoke()");
      }
    }
  }
}


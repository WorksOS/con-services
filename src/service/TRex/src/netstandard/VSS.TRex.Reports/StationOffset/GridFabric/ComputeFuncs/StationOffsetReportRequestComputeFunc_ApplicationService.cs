using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.Reports.StationOffset.Executors;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;

namespace VSS.TRex.Reports.StationOffset.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The StationOffset compute function responsible for coordinating sub grids comprising a patch a server compute node in response to 
  /// a client server instance requesting it.
  /// </summary>
  public class StationOffsetReportRequestComputeFunc_ApplicationService : BaseComputeFunc, IComputeFunc<StationOffsetReportRequestArgument_ApplicationService, StationOffsetReportRequestResponse_ApplicationService>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<StationOffsetReportRequestComputeFunc_ApplicationService>();

    public StationOffsetReportRequestResponse_ApplicationService Invoke(StationOffsetReportRequestArgument_ApplicationService arg)
    {
      Log.LogInformation($"Start {nameof(StationOffsetReportRequestComputeFunc_ApplicationService)}");

      try
      {
        var executor = new ComputeStationOffsetReportExecutor_ApplicationService();
        return executor.Execute(arg);
      }
      finally
      {
        Log.LogInformation($"End {nameof(StationOffsetReportRequestComputeFunc_ApplicationService)}");
      }
    }
  }
}

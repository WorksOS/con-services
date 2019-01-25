using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.Alignments;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using VSS.TRex.Types;

namespace VSS.TRex.Reports.StationOffset.Executors
{
  /// <summary>
  /// Executes business logic that generates a list of points
  ///     along the station at requested intervals and offsets
  ///     from the alignment design
  /// </summary>
  public class ComputeStationOffsetReportExecutor_ApplicationService : SubGridsPipelinedResponseBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ComputeStationOffsetReportExecutor_ApplicationService>();

    public ComputeStationOffsetReportExecutor_ApplicationService()
    { }

    /// <summary>
    /// Executes the profiler
    /// </summary>
    public StationOffsetReportRequestResponse_ApplicationService Execute(StationOffsetReportRequestArgument_ApplicationService arg)
    {
      Log.LogInformation($"Start {nameof(ComputeStationOffsetReportExecutor_ApplicationService)}");

      try
      {
        if (arg.Filters?.Filters != null && arg.Filters.Filters.Length > 0)
        {
          // Prepare the filters for use in stationOffset operations. Failure to prepare any filter results in this request terminating
          if (!(arg.Filters.Filters.Select(x => FilterUtilities.PrepareFilterForUse(x, arg.ProjectID)).All(x => x == RequestErrorStatus.OK)))
          {
            return new StationOffsetReportRequestResponse_ApplicationService { ResultStatus = RequestErrorStatus.FailedToPrepareFilter};
          }
        }

        // keep alignment design knowledge here and pass points
        var argClusterCompute = new StationOffsetReportRequestArgument_ClusterCompute
        {
          ProjectID = arg.ProjectID,
          Filters = arg.Filters,
          ReferenceDesignUID = arg.ReferenceDesignUID,
          TRexNodeID = arg.TRexNodeID,
          ReportElevation = arg.ReportElevation,
          ReportCmv = arg.ReportCmv,
          ReportMdp = arg.ReportMdp,
          ReportPassCount = arg.ReportPassCount,
          ReportTemperature = arg.ReportTemperature,
          ReportCutFill = arg.ReportCutFill,
          Points = new List<StationOffsetPoint>()
        };

        var alignmentDesign = DIContext.Obtain<IAlignmentManager>().List(argClusterCompute.ProjectID).Locate(arg.AlignmentDesignUid);
        argClusterCompute.Points = alignmentDesign.GetOffsetPointsInNEE(arg.CrossSectionInterval, arg.StartStation, arg.EndStation, arg.Offsets);
        Log.LogInformation($"{nameof(StationOffsetReportRequestResponse_ApplicationService)}: pointCount: {argClusterCompute.Points.Count}");
        // todoJeannie what if zero points?

        var request = new StationOffsetReportRequest_ClusterCompute();
        var clusterComputeResponse = request.Execute(argClusterCompute);
        
        // Return the core package to the caller
        var applicationResponse = new StationOffsetReportRequestResponse_ApplicationService();
        applicationResponse.LoadStationOffsets(clusterComputeResponse.StationOffsetRows);
        return applicationResponse;
      }
      finally
      {
        Log.LogInformation($"End {nameof(ComputeStationOffsetReportExecutor_ApplicationService)}");
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Requests;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.Reports.StationOffset.Executors
{
  /// <summary>
  /// Executes business logic that generates a list of points
  ///     along the station at requested intervals and offsets
  ///     from the alignment design
  /// </summary>
  public class ComputeStationOffsetReportExecutor_ApplicationService // : SubGridsPipelinedResponseBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ComputeStationOffsetReportExecutor_ApplicationService>();

    public ComputeStationOffsetReportExecutor_ApplicationService()
    {
    }

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
            return new StationOffsetReportRequestResponse_ApplicationService {ResultStatus = RequestErrorStatus.FailedToPrepareFilter};
          }
        }

        // keep alignment design knowledge here and pass points
        var argClusterCompute = new StationOffsetReportRequestArgument_ClusterCompute
        {
          ProjectID = arg.ProjectID,
          Filters = arg.Filters,
          ReferenceDesign.DesignID = arg.ReferenceDesign.DesignID,
          TRexNodeID = arg.TRexNodeID,
          ReportElevation = arg.ReportElevation,
          ReportCmv = arg.ReportCmv,
          ReportMdp = arg.ReportMdp,
          ReportPassCount = arg.ReportPassCount,
          ReportTemperature = arg.ReportTemperature,
          ReportCutFill = arg.ReportCutFill,
          Points = new List<StationOffsetPoint>()
        };

        // alignment sdk will convert interval/offsets into northing/eastings for the project
        // todo temporarily get mock points until alignment SDK available
        // var alignmentDesign = DIContext.Obtain<IAlignmentManager>().List(argClusterCompute.ProjectID).Locate(arg.AlignmentDesignUid);
        // argClusterCompute.Points = alignmentDesign.GetOffsetPointsInNEE(arg.CrossSectionInterval, arg.StartStation, arg.EndStation, arg.Offsets);

        argClusterCompute.Points = GetMockPointsFromSiteModel(argClusterCompute.ProjectID, 3);

        Log.LogInformation($"{nameof(StationOffsetReportRequestResponse_ApplicationService)}: pointCount: {argClusterCompute.Points.Count}");
        if (argClusterCompute.Points.Count == 0)
        {
          return new StationOffsetReportRequestResponse_ApplicationService() {ReturnCode = ReportReturnCode.NoData, ResultStatus = RequestErrorStatus.NoProductionDataFound};
        }

        var request = new StationOffsetReportRequest_ClusterCompute();
        var clusterComputeResponse = request.Execute(argClusterCompute);

        // Return the core package to the caller
        var applicationResponse = new StationOffsetReportRequestResponse_ApplicationService()
          { ReturnCode = clusterComputeResponse.ReturnCode,
            ResultStatus = clusterComputeResponse.ResultStatus
          };
        applicationResponse.LoadStationOffsets(clusterComputeResponse.StationOffsetRows);
        Log.LogInformation($"End {nameof(ComputeStationOffsetReportExecutor_ApplicationService)}: ReturnCode {applicationResponse.ReturnCode}.");
        return applicationResponse;
      }
      catch (Exception e)
      {
        Log.LogError(e, $"{nameof(StationOffsetReportRequestResponse_ApplicationService)}: Unexpected exception.");
        throw;
      }

    }

    // todo temp until alignment SDK available
    private List<StationOffsetPoint> GetMockPointsFromSiteModel(Guid projectUid, int countPointsRequired)
    {
      var points = new List<StationOffsetPoint>();
      double station = 0;
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid);

      /* this piece of code reads data into memory, using the existence map */
      siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(address =>
      {
        if (points.Count > countPointsRequired)
          return;

        var subGrid = SubGridUtilities.LocateSubGridContaining
        (siteModel.PrimaryStorageProxy, siteModel.Grid, address.X, address.Y, siteModel.Grid.NumLevels, false, false);

        if (subGrid != null)
        {
          subGrid.CalculateWorldOrigin(out var originX, out var originY);

          ((IServerLeafSubGrid) subGrid).Directory.GlobalLatestCells.PassDataExistenceMap.ForEachSetBit(
            (x, y) =>
            {
              points.Add(new StationOffsetPoint(station += 1, 0,
                originY + y * siteModel.CellSize + siteModel.CellSize / 2,
                originX + x * siteModel.CellSize + siteModel.CellSize / 2));
              return points.Count < countPointsRequired;
            });
        }
      });

      return points;
    }
  }
}

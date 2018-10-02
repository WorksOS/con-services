using ASNodeDecls;
using ASNodeRaptorReports;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using Newtonsoft.Json;
using SVOICDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// The executor, which passes the report grid request to Raptor.
  /// </summary>
  public class CompactionReportStationOffsetExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the report grid request by passing the request to Raptor and returning the result.
    /// </summary>
    /// <returns>Returns an instance of the <see cref="CompactionReportResult"/> class if successful.</returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;

      try
      {
        var request = item as CompactionReportStationOffsetRequest;

        if (request == null)
          ThrowRequestTypeCastException<CompactionReportStationOffsetRequest>();

        var filterSettings = RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.ProjectId);
        var cutfillDesignDescriptor = RaptorConverters.DesignDescriptor(request.DesignFile);
        var alignmentDescriptor = RaptorConverters.DesignDescriptor(request.AlignmentFile);
        var userPreferences = ExportRequestHelper.ConvertUserPreferences(request.UserPreferences, request.ProjectTimezone);

        var options = RaptorConverters.convertOptions(null, request.LiftBuildSettings, 0,
          request.Filter?.LayerType ?? FilterLayerMethod.None, DisplayMode.Height, false);

        log.LogDebug("About to call GetReportStationOffset");

        var args = ASNode.StationOffsetReport.RPC.__Global.Construct_StationOffsetReport_Args(
          request.ProjectId ?? -1,
          (int)CompactionReportType.StationOffset,
          ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0, TASNodeCancellationDescriptorType.cdtProdDataReport),
          userPreferences,
          alignmentDescriptor,
          cutfillDesignDescriptor,
          request.StartStation,
          request.EndStation,
          request.Offsets,
          request.CrossSectionInterval,
          request.ReportElevation,
          request.ReportCutFill,
          request.ReportCMV,
          request.ReportMDP,
          request.ReportPassCount,
          request.ReportTemperature,
          (int)GridReportOption.Unused,
          0, 0, 0, 0, 0, 0, 0, // Northings, Eastings and Direction values are not used on Station Offset report.
          filterSettings,
          RaptorConverters.ConvertLift(request.LiftBuildSettings, filterSettings.LayerMethod),
          options
        );

        int returnedResult = raptorClient.GetReportStationOffset(args, out var responseData);

        log.LogDebug("Completed call to GetReportStationOffset");

        var success = 1;

        if (returnedResult != success)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
              "Failed to get requested station offset report data"));
        }

        try
        {
          // Unpack the data for the report and construct a stream containing the result
          TRaptorReportsPackager reportPackager = new TRaptorReportsPackager(TRaptorReportType.rrtStationOffset)
          {
            ReturnCode = TRaptorReportReturnCode.rrrcUnknownError
          };

          log.LogDebug("Retrieving response data");

          reportPackager.ReadFromStream(responseData);

          var stationRows = new StationRow[reportPackager.StationOffsetReport.NumberOfStations];

          for (var i = 0; i < reportPackager.StationOffsetReport.NumberOfStations; i++)
          {
            var station = reportPackager.StationOffsetReport.Stations[i];
            var stationRow = StationRow.Create(station, request);

            for (var j = 0; j < station.NumberOfOffsets; j++)
            {
              stationRow.Offsets[j] = StationOffsetRow.CreateRow(station.Offsets[j], request);
            }

            stationRows[i] = stationRow;
          }

          var startAndEndTime = request.Filter.StartUtc ?? DateTime.Now;
          var stationOffsetReport = StationOffsetReport.CreateReport(startAndEndTime, startAndEndTime, stationRows, request);

          result = CompactionReportResult.CreateExportDataResult(stationOffsetReport, (short)returnedResult);
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.NoContent,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Failed to retrieve received station offset report data: " + ex.Message));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }

      return result;
    }
  }
}

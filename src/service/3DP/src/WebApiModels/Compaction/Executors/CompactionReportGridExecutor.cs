using ASNodeDecls;
using ASNodeRaptorReports;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Reports;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Processes the request to get grid details.
  /// </summary>
  public class CompactionReportGridExecutor : RequestExecutorContainer
  {
    public CompactionReportGridExecutor()
    {
      ProcessErrorCodes();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = item as CompactionReportGridRequest;

        if (request == null)
          ThrowRequestTypeCastException<CompactionReportGridRequest>();

        bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_STATIONOFFSET"), out var useTrexGateway);

        MemoryStream responseData;
        var success = 1;

        if (useTrexGateway)
        {
          responseData = (MemoryStream) trexCompactionDataProxy.SendGridReportRequest(request, customHeaders).Result;
        }
        else
        {
          var raptorFilter = RaptorConverters.ConvertFilter(request.FilterID, request.Filter, request.ProjectId);

          var options = RaptorConverters.convertOptions(null, request.LiftBuildSettings, 0,
            request.Filter?.LayerType ?? FilterLayerMethod.None, DisplayMode.Height, false);

          log.LogDebug("About to call GetReportGrid");

          var args = ASNode.GridReport.RPC.__Global.Construct_GridReport_Args(
            request.ProjectId ?? -1,
            (int) CompactionReportType.Grid,
            ASNodeRPC.__Global.Construct_TASNodeRequestDescriptor(Guid.NewGuid(), 0,
              TASNodeCancellationDescriptorType.cdtProdDataReport),
            RaptorConverters.DesignDescriptor(request.DesignFile),
            request.GridInterval,
            request.ReportElevation,
            request.ReportCutFill,
            request.ReportCMV,
            request.ReportMDP,
            request.ReportPassCount,
            request.ReportTemperature,
            (int) request.GridReportOption,
            request.StartNorthing,
            request.StartEasting,
            request.EndNorthing,
            request.EndEasting,
            request.Azimuth,
            raptorFilter,
            RaptorConverters.ConvertLift(request.LiftBuildSettings, raptorFilter.LayerMethod),
            options
          );

          int returnedResult = raptorClient.GetReportGrid(args, out responseData);

          if (returnedResult != success)
            throw CreateServiceException<CompactionReportGridExecutor>();
        }

        log.LogDebug("Completed call to GetReportGrid");
        try
        {
          // Unpack the data for the report and construct a stream containing the result
          var reportPackager = new TRaptorReportsPackager(TRaptorReportType.rrtGridReport)
          {
            ReturnCode = TRaptorReportReturnCode.rrrcUnknownError
          };

          reportPackager.GridReport.ElevationReport = request.ReportElevation;
          reportPackager.GridReport.CutFillReport = request.ReportCutFill;
          reportPackager.GridReport.CMVReport = request.ReportCMV;
          reportPackager.GridReport.MDPReport = request.ReportMDP;
          reportPackager.GridReport.PassCountReport = request.ReportPassCount;
          reportPackager.GridReport.TemperatureReport = request.ReportTemperature;

          log.LogDebug("Retrieving response data");

          reportPackager.ReadFromStream(responseData);

          var gridRows = new GridRow[reportPackager.GridReport.NumberOfRows];

          // Populate an array of grid rows from the data
          //foreach (TGridRow row in reportPackager.GridReport.Rows)
          for (var i = 0; i < reportPackager.GridReport.NumberOfRows; i++)
          {
            gridRows[i] = GridRow.CreateRow(reportPackager.GridReport.Rows[i], request);
          }

          var startTime = request.Filter != null && request.Filter.StartUtc.HasValue
            ? request.Filter.StartUtc.Value
            : DateTime.Now;
          var endTime = request.Filter != null && request.Filter.EndUtc.HasValue
            ? request.Filter.EndUtc.Value
            : DateTime.Now;

          var gridReport = GridReport.CreateGridReport(startTime, endTime, gridRows);

          return CompactionReportResult.CreateExportDataResult(gridReport, (short)success);
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.NoContent,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Failed to retrieve received grid report data: " + ex.Message));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}

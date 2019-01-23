using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting report data.
  /// </summary>
  public class ReportsController : BaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    public ReportsController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<DetailsDataController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get station-offset report stream for the specified project, filter etc.
    /// </summary>
    /// <param name="reportStationOffsetRequest"></param>
    /// <returns></returns>
    [Route("api/v1/report/stationoffset")]
    [HttpPost]
    public FileResult PostStationOffsetReport([FromBody] CompactionReportStationOffsetTRexRequest reportStationOffsetRequest)
    {
      Log.LogInformation($"{nameof(PostStationOffsetReport)}: {Request.QueryString}");

      reportStationOffsetRequest.Validate();
      ValidateDataAvailable(reportStationOffsetRequest.ProjectUid, reportStationOffsetRequest.CutFillDesignUid, reportStationOffsetRequest.AlignmentDesignUid);
      
      var stationOffsetReportDataResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<StationOffsetReportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(reportStationOffsetRequest) as GriddedReportDataResult);

      if (stationOffsetReportDataResult?.GriddedData == null)
      {
        var code = stationOffsetReportDataResult == null ? HttpStatusCode.BadRequest : HttpStatusCode.NoContent;
        var exCode = stationOffsetReportDataResult == null ? ContractExecutionStatesEnum.FailedToGetResults : ContractExecutionStatesEnum.ValidationError;

        throw new ServiceException(code, new ContractExecutionResult(exCode, $"Failed to get stationOffset report data for projectUid: {reportStationOffsetRequest.ProjectUid}"));
      }

      return new FileStreamResult(new MemoryStream(stationOffsetReportDataResult?.GriddedData), "application/octet-stream");
    }

    /// <summary>
    /// Get grid report for the specified project, filter etc.
    /// </summary>
    /// <param name="reportGridRequest"></param>
    /// <returns></returns>
    [Route("api/v1/report/grid")]
    [HttpPost]
    public FileResult PostGriddedReport([FromBody] CompactionReportGridTRexRequest reportGridRequest)
    {
      Log.LogInformation($"{nameof(PostGriddedReport)}: {Request.QueryString}");

      reportGridRequest.Validate();
      ValidateDataAvailable(reportGridRequest.ProjectUid, reportGridRequest.CutFillDesignUid, null);

      var griddedReportDataResult =  WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<GriddedReportExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(reportGridRequest) as GriddedReportDataResult);

      if (griddedReportDataResult?.GriddedData == null)
      {
        var code = griddedReportDataResult == null ? HttpStatusCode.BadRequest : HttpStatusCode.NoContent;
        var exCode = griddedReportDataResult == null ? ContractExecutionStatesEnum.FailedToGetResults : ContractExecutionStatesEnum.ValidationError;

        throw new ServiceException(code, new ContractExecutionResult(exCode, $"Failed to get gridded report data for projectUid: {reportGridRequest.ProjectUid}"));
      }

      return new FileStreamResult(new MemoryStream(griddedReportDataResult?.GriddedData), "application/octet-stream");
    }

    private void ValidateDataAvailable(Guid projectUid, Guid? cutFillDesignUid, Guid? alignmentUid)
    {
      if (DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid, false) == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Project: {projectUid} is not found."));
      }

      if (cutFillDesignUid.HasValue &&
          DIContext.Obtain<IDesignManager>().List(projectUid).Locate(cutFillDesignUid.Value) == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"CutFill design {cutFillDesignUid.Value} is not found."));
      }

      if (alignmentUid.HasValue &&
          DIContext.Obtain<IAlignmentManager>().List(projectUid).Locate(alignmentUid.Value) == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Alignment design {alignmentUid} is not found."));
      }
    }
  }
}

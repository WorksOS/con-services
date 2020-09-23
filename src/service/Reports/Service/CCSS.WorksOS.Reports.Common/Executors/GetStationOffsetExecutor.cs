using System;
using System.Net;
using System.Threading.Tasks;
using CCSS.WorksOS.Reports.Abstractions.Models.Request;
using CCSS.WorksOS.Reports.Common.DataGrabbers;
using CCSS.WorksOS.Reports.Common.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.WorksOS.Reports.Common.Executors
{
  /// <summary>
  /// The executor which gets the project settings for the project
  /// </summary>
  public class GetStationOffsetExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the report request including: obtaining data, formatting the report and creating it in S3
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var result = new ContractExecutionResult((int) HttpStatusCode.InternalServerError, "failed");
      var reportRequest = CastRequestObjectTo<ReportRequest>(item, errorCode: 9);

      /* todoJeannie
      getUserPreferences()
      dataGrabber()
      formatXLSXStationOffsetReport()
      WriteReportToS3()
      formatResponse()
      */

      try
      {
        var userPreferences = await _preferenceProxy.GetUserPreferences(_userUid, _customHeaders);

        var composerRequest = new GenericComposerRequest()
        {
          UserUid = _userUid,
          CustomerUID = _customerUid,
          CustomHeaders = _customHeaders,
          UserPreference = userPreferences,
          ReportRequest = reportRequest
        };

        var dataGrabber = new StationOffsetDataGrabber(_log, _serviceExceptionHandler, _gracefulClient, composerRequest);
        var dataGrabberResponse = dataGrabber.GetReportsData();

      }
      catch (Exception e)
      {
        _serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 9, e.Message);
      }
      return result;
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}

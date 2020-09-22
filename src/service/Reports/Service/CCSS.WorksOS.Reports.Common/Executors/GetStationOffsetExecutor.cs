using System;
using System.Net;
using System.Threading.Tasks;
using CCSS.WorksOS.Reports.Abstractions.Models.Request;
using CCSS.WorksOS.Reports.Common.Helpers;
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

        var dataGrabber = DataGrabberHelper.CreateDataGrabber(_log, _serviceExceptionHandler, _gracefulClient, composerRequest);
        var dataGrabberResponse = dataGrabber.GetReportsData();

        //if (ProcessDirectDownloadMultipleReport(request, ReportUID, decodedJWT.UserUid.ToString(), CustomerUid, UserCusResp.UserCustomerId, bearerToken, userPreference))
        //{
        //  string downloadURLFormat = PublicApiBaseUrl + DirectDownloadUrl;
        //  var culture = (userPreference != null && userPreference.Language != null) ? userPreference.Language : Constants.DEFAULT_LANGUAGE;
        //  string textToEncrypt_DownloadURL = Constants.REPORTID + "=" + ReportUID + "&" + Constants.LOCALE + "=" + culture;
        //  string EncryptedDownloadURL = string.Format(downloadURLFormat, HttpUtility.UrlEncode(EncryptionUtilities.EncryptText(textToEncrypt_DownloadURL, DirectReportEncryptionKey)));
        //  return ResponseMessage(Request.CreateResponse(HttpStatusCode.Created, (new CreateDirectReportSuccessResponse
        //  {
        //    RequestId = requestContext.Id.ToString(),
        //    Status = HttpStatusCode.Created,
        //    Message = Messages.Report_Generated_Success,
        //    ReportUid = ReportUID,
        //    DownloadLink = EncryptedDownloadURL
        //  })));


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

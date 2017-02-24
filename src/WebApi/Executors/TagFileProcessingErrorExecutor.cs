using System.Net;
using VSS.TagFileAuth.Service.WebApi.Enums;
using VSS.TagFileAuth.Service.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.ResultHandling;
using VSS.TagFileAuth.Service.WebApi.Interfaces;

namespace VSS.TagFileAuth.Service.Executors
{
  /// <summary>
  /// The executor which sends an alert if required for a tag file processing error.
  /// </summary>
  public class TagFileProcessingErrorExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the tag file processing error request and creates an alert if required.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a TagFileProcessingErrorResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      TagFileProcessingErrorRequest request = item as TagFileProcessingErrorRequest;

      bool result = false;

      if (request.assetId > 0)
      {
        //Create an alert if required for request.assetId, request.tagFileName and request.error.

        //In VisionLink the user can set up an alert for these errors. Since there may be a lot of them
        //he can set the frequency of reporting of the alert in hours (e.g. 6 hours) and we accummulate
        //them and report one alert for the interval with the first and last occurence date time and the
        //number of incidents
        /*
            request.error         alert type
         -2	UnknownProject	      UnableToDetermineProjectID
         -1 UnknownCell	          NoValidCellPassesInTagfile
          1	NoMatchingProjectDate	UnableToDetermineProjectID
          2	NoMatchingProjectArea	UnableToDetermineProjectID
          3	MultipleProjects	    UnableToDetermineProjectID
          4	InvalidSeedPosition	  UnableToDetermineProjectID
          5	InvalidOnGroundFlag	  NoValidCellPassesInTagfile
          6	InvalidPosition	      NoValidCellPassesInTagfile
         */

         result = request.error == TagFileErrorsEnum.None;
      }

      try
      {
        return TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(result);
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to create an alert for tag file processing error"));
      }

    }

    //protected override void ProcessErrorCodes()
    //{
    //  //Nothing to do
    //}
  }
}
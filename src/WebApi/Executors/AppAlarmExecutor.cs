using System.Net;
using VSS.TagFileAuth.Service.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.ResultHandling;
using VSS.TagFileAuth.Service.WebApi.Interfaces;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.Executors
{
  /// <summary>
  /// The executor which raises an application alarm.
  /// </summary>
  public class AppAlarmExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the application alarm request and raises an application alarm.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>an AppAlarmResult if successful</returns>      
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      AppAlarmRequest request = item as AppAlarmRequest;

      bool result = false;

      if (request.alarmType == TSigLogMessageClass.slmcAssert || request.alarmType == TSigLogMessageClass.slmcError || request.alarmType == TSigLogMessageClass.slmcException)
      {
        //Raise an application alarm using request.message and request.exceptionMessage
      }
      else
      {
        //Perhaps Log the request
      }

      result = true;

      if (true)//determine here if successful
      {
        return AppAlarmResult.CreateAppAlarmResult(result);
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to raise an application alarm"));
      }

    }

    //protected override void ProcessErrorCodes()
    //{
    //  //Nothing to do
    //}
  }
}
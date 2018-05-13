using System;
using System.Linq;
using System.Net;
using Hangfire.Common;
using Hangfire.States;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Used to add custom export failure information to be used in the 'get status' requests
  /// </summary>
  public class ExportFailureFilter : JobFilterAttribute, IElectStateFilter
  {
    /// <summary>
    /// 
    /// </summary>
    public void OnStateElection(ElectStateContext context)
    {
      // the way Hangfire works is retrying a job X times (10 by default), 
      //so this won't be called directly with a failed state sometimes.
      // To solve this we should look into TraversedStates for a failed state

      var failed = context.CandidateState as FailedState ??
                   context.TraversedStates.FirstOrDefault(x => x is FailedState) as FailedState;

      if (failed == null)
        return;

      //GracefulWebRequest gets a HttpWebResponse containing the ServiceException and formats it as the message of a System.Exception
      //e.g. "BadRequest {"Code":2002,"Message":"Failed to get requested export data with error: No data for export"}"

      var message = failed.Exception.Message;
      var httpStatusCode = HttpStatusCode.InternalServerError;
      ContractExecutionResult result = null;
      try
      {
        //Extract the HttpStatusCode
        httpStatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), message.Split(' ')[0]);
        message = message.Substring(httpStatusCode.ToString().Length + 1);
        //See if it's a service exception
        result = JsonConvert.DeserializeObject<ContractExecutionResult>(message);
      }
      catch (Exception)
      {
        //Not a service exception therefore just use original exception message
        result = new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, message);
      }

      var failedDetails = new FailureDetails { Code = httpStatusCode, Result = result };
      var additionalData = JsonConvert.SerializeObject(failedDetails);
      context.CandidateState = new ExportFailedState(failed.Exception, additionalData);
    }
  }
}

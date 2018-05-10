using System.Linq;
using Hangfire.Common;
using Hangfire.States;
using Newtonsoft.Json;
using VSS.Common.Exceptions;

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

      //here you have the failed.Exception and you can do anything with it
      //and also the job name context.Job.Type.Name

      //Check for 400 and 500 errors which come through as an inner exception
      var ex = failed.Exception.InnerException ?? failed.Exception;

      string additionalData = string.Empty;
      if (ex is ServiceException)
      {
        additionalData = JsonConvert.SerializeObject(failed.Exception as ServiceException);
      }

      context.CandidateState = new ExportFailedState(failed.Exception, additionalData);
    }
  }
}

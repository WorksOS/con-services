using System.Linq;
using Hangfire.Common;
using Hangfire.States;
using VSS.Productivity3D.Scheduler.WebAPI.JobRunner;

namespace VSS.Productivity3D.Scheduler.WebApi.JobRunner
{
  /// <summary>
  /// Used to notify developers of job failures
  /// </summary>
  public class JobFailureFilter : JobFilterAttribute, IElectStateFilter
  {
    /// <summary>
    /// Called by Hangfire when the state of the job changes
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

      //Add our custom information for DevOps notification to be saved in the database for future reference
      context.CandidateState = new JobFailedState(failed.Exception, failed.Exception.JobNotificationDetails(context.BackgroundJob.Id));
    }
  }
}

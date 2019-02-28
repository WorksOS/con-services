using System;
using VSS.Common.Exceptions;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  public static class ExceptionExtensions
  {
    /// <summary>
    /// Construct details for a job failure notification
    /// </summary>
    public static string JobNotificationDetails(this Exception e, string jobId)
    {
      var message = e.Message;
      if (e is ServiceException)
      {
        message = (e as ServiceException).GetFullContent;
      }
      var stackTrace = e.StackTrace;
      return $"jobid: {jobId}\nmessage: {message}\nstacktrace: {stackTrace}";
    }
  }
}

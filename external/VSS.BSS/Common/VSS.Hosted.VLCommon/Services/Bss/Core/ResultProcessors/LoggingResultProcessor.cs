using log4net;

namespace VSS.Hosted.VLCommon.Bss
{
  public class LoggingResultProcessor : IWorkflowResultProcessor
  {
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public void Process<TMessage>(TMessage sourceMessage, WorkflowResult workflowResult)
    {
      foreach (var activityResult in workflowResult.ActivityResults)
      {
        switch (activityResult.Type)
        {
          case ResultType.Debug:
            Log.DebugFormat("Executed:{0} {1}", activityResult.DateTimeUtc, activityResult.Summary);
            break;

          case ResultType.Information:

            Log.InfoFormat("Executed:{0} {1}", activityResult.DateTimeUtc, activityResult.Summary);
            break;

          case ResultType.Warning:

            Log.WarnFormat("Executed:{0} {1}", activityResult.DateTimeUtc, activityResult.Summary);
            break;

          case ResultType.Error:

            var errorResult = activityResult as BssErrorResult;
            if(errorResult != null)
            {
              Log.WarnFormat("{0} Executed:{1} Failure Code: {2} {3}", "BSS Error!", errorResult.DateTimeUtc, errorResult.FailureCode, errorResult.Summary);
              break;
            }

            Log.ErrorFormat("Executed:{0} {1}", activityResult.DateTimeUtc, activityResult.Summary);
            break;

          case ResultType.Exception:

            var exceptionResult = activityResult as ExceptionResult;
            if(exceptionResult != null)
            {
              Log.Error(string.Format("Executed:{0} {1}", exceptionResult.DateTimeUtc, exceptionResult.Summary), exceptionResult.Exception);
            }
            break;

          case ResultType.Notify:

            var notifyResult = activityResult as NotifyResult;
            if (notifyResult != null)
            {
              Log.Error(string.Format("Executed:{0} {1}", notifyResult.DateTimeUtc, notifyResult.Summary), notifyResult.Exception);
            }
            break;

          default:
            Log.Warn("Unhandled result type.");
            break;
        }
      }
    }
  }
}

using log4net;
using System;
using System.Text;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BatchLoggingResultProcessor : IWorkflowResultProcessor
  {
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public void Process<TMessage>(TMessage sourceMessage, WorkflowResult workflowResult)
    {
      var severity = ResultType.Information;
      Exception exception = null;

      var sb = GetActivityResultAsString(sourceMessage, workflowResult, ref severity, ref exception);

      switch (severity)
      {
        case ResultType.Error:
        case ResultType.Exception:

          if(exception != null)
            Log.Error(sb.ToString(), exception);
          else
            Log.Error(sb.ToString());
          break;

        case ResultType.Warning:
        case ResultType.Notify:

          Log.Warn(sb.ToString());
          break;

        case ResultType.Information:

          Log.Info(sb.ToString());
          break;

        default:

          Log.Debug(sb.ToString());
          break;
      }
    }

    private StringBuilder GetActivityResultAsString<TMessage>(TMessage sourceMessage, WorkflowResult workflowResult, ref ResultType severity, ref Exception exception)
    {
      var sb = new StringBuilder();

      sb.AppendLine();
      sb.AppendFormat("{0} {1} {0}{2}", "************", typeof(TMessage).Name, Environment.NewLine);
      sb.AppendFormat("Source Message: {0}{1}", sourceMessage.DataContractObjectToXml(), Environment.NewLine);

      foreach (var result in workflowResult.ActivityResults)
      {
        switch (result.Type)
        {
          case ResultType.Debug:

            sb.AppendFormat("{0} DEBUG {1}{2}", result.DateTimeUtc, result.Summary, Environment.NewLine);
            break;

          case ResultType.Information:

            sb.AppendFormat("{0} INFO {1}{2}", result.DateTimeUtc, result.Summary, Environment.NewLine);
            break;

          case ResultType.Warning:

            severity = ResultType.Warning;
            sb.AppendFormat("{0} WARNING {1}{2}", result.DateTimeUtc, result.Summary, Environment.NewLine);
            break;

          case ResultType.Error:

            var errorResult = result as BssErrorResult;
            if (errorResult != null)
            {
              severity = ResultType.Warning;
              sb.AppendFormat("{0} BSS ERROR FailureCode:{1} {2}{3}", errorResult.DateTimeUtc, errorResult.FailureCode, errorResult.Summary, Environment.NewLine);
              break;
            }

            severity = ResultType.Error;
            sb.AppendFormat("{0} ERROR {1}{2}", result.DateTimeUtc, result.Summary, Environment.NewLine);
            break;

          case ResultType.Exception:

            var exceptionResult = result as ExceptionResult;
            if (exceptionResult != null)
            {
              severity = ResultType.Exception;
              exception = exceptionResult.Exception;

              sb.AppendFormat("{0} EXCEPTION {1}{2}", exceptionResult.DateTimeUtc, exceptionResult.Summary, Environment.NewLine);
            }
            break;

          case ResultType.Notify:

            var notifyResult = result as NotifyResult;
            if (notifyResult != null)
            {
              severity = ResultType.Notify;
              exception = notifyResult.Exception;

              sb.AppendFormat("{0} NOTIFY {1}{2}", notifyResult.DateTimeUtc, notifyResult.Summary, Environment.NewLine);
            }
            break;

          default:

            severity = ResultType.Warning;
            sb.AppendLine("Unhandled result type.");
            break;
        }
      }

      sb.AppendFormat("{0} {1} {0}", "************", "END");
      return sb;
    }
  }
}
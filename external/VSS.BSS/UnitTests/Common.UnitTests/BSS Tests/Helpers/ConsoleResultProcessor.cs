using System;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  public class ConsoleResultProcessor : IWorkflowResultProcessor 
  {
    public void Process<TMessage>(TMessage message, WorkflowResult workflowResult)
    {
      Console.WriteLine("Source Message:\n{0}", message.DataContractObjectToXml());

      Console.WriteLine("Success: {0}", workflowResult.Success);

      foreach (var activityResult in workflowResult.ActivityResults)
      {
        switch (activityResult.Type)
        {
          case ResultType.Debug:

            Console.WriteLine("Executed:{0} {1}", activityResult.DateTimeUtc, activityResult.Summary);
            break;

          case ResultType.Information:

            Console.WriteLine("Executed:{0} {1}", activityResult.DateTimeUtc, activityResult.Summary);
            break;

          case ResultType.Warning:

            Console.WriteLine("Executed:{0} {1}", activityResult.DateTimeUtc, activityResult.Summary);
            break;

          case ResultType.Error:

            var result = activityResult as BssErrorResult;
            if (result != null)
            {
              var bssResult = result;
              Console.WriteLine("{0} Executed:{1} Failure Code: {2} {3}", "BSS Error!", bssResult.DateTimeUtc, bssResult.FailureCode, bssResult.Summary);
              break;
            }

            Console.WriteLine("ERROR:{0} {1}", activityResult.DateTimeUtc, activityResult.Summary);
            break;

          case ResultType.Exception:
            var exceptionResult = activityResult as ExceptionResult;
            if (exceptionResult != null)
            {
              Console.WriteLine("{0} Exception: {1} {2}\nStackTrace:{3}", exceptionResult.DateTimeUtc,
                exceptionResult.GetType().Name, exceptionResult.Exception.Message, exceptionResult.Exception.StackTrace);

              if (exceptionResult.Exception.InnerException != null)
              {
                Console.WriteLine("\nInnerException: {0} {1}\nStackTrace:{2}", exceptionResult.GetType().Name, 
                  exceptionResult.Exception.InnerException.Message, exceptionResult.Exception.InnerException.StackTrace);
              }
            }
            break;

          case ResultType.Notify:
            var notifyResult = activityResult as NotifyResult;
            if (notifyResult != null)
            {
              Console.WriteLine("{0} Exception: {1} {2}\nStackTrace:{3}", notifyResult.DateTimeUtc,
                notifyResult.GetType().Name, notifyResult.Exception.Message, notifyResult.Exception.StackTrace);

              if (notifyResult.Exception.InnerException != null)
              {
                Console.WriteLine("\nInnerException: {0} {1}\nStackTrace:{2}", notifyResult.GetType().Name,
                  notifyResult.Exception.InnerException.Message, notifyResult.Exception.InnerException.StackTrace);
              }
            }
            break;

          default:
            Console.WriteLine("Unhandled result type.");
            break;
        }
      }
    }
  }
}
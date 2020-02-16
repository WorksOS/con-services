using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class LoggingResultProcessorTests : BssUnitTestBase
  {
    [TestMethod]
    public void Process_WritesToLogFile()
    {
      var workflowResult = new WorkflowResult();
      var debug = new DebugResult {Summary = "Debug result"};
      var info = new ActivityResult {Summary = "Activity result"};
      var warn = new WarningResult {Summary = "Warning result"};
      var bssError = new BssErrorResult {Summary = "BssError result", FailureCode = BssFailureCode.MessageInvalid};
      var error = new ErrorResult {Summary = "Error result"};
      var exception = new ExceptionResult {Summary = "Exception result", Exception = new InvalidOperationException("Exception", new Exception("InnerException"))};
      var notify = new NotifyResult { Summary = "Exception result", Exception = new InvalidOperationException("Exception", new Exception("InnerException")) };
      
      workflowResult.ActivityResults.Add(debug);
      workflowResult.ActivityResults.Add(info);
      workflowResult.ActivityResults.Add(warn);
      workflowResult.ActivityResults.Add(bssError);
      workflowResult.ActivityResults.Add(error);
      workflowResult.ActivityResults.Add(exception);
      workflowResult.ActivityResults.Add(notify);

      var batchLogger = new BatchLoggingResultProcessor();
      var testProcessor = new ConsoleResultProcessor();

      batchLogger.Process(new AccountHierarchy(), workflowResult);
      testProcessor.Process(new AccountHierarchy(), workflowResult);
    }
  }
}

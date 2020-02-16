using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class WorkflowRunnerTests : BssUnitTestBase
  {
    protected WorkflowResult WorkflowResult;

    [TestInitialize]
    public void WorkflowRunnerTests_Init()
    {
      if (WorkflowResult == null) return;
      new ConsoleResultProcessor().Process((AccountHierarchy)null, WorkflowResult);
    }
    
    [TestMethod]
    public void Run_NoActivitySequences_ReturnsSuccessFalseResult()
    {
      IWorkflow workflow = new WorkflowFake();

      WorkflowResult = new WorkflowRunner().Run(workflow);

      Assert.IsFalse(WorkflowResult.Success);
      Assert.AreEqual(string.Format(CoreConstants.WORKFLOW_FAILED, CoreConstants.WORKFLOW_HAS_NO_ACTIVITY_SEQUENCES), WorkflowResult.Summary);
      Assert.AreEqual(0, WorkflowResult.ActivityResults.Count);
    }

    [TestMethod]
    public void Run_AllActivitySequencesExecuted_ReturnsSuccessTrueResult()
    {
      IWorkflow workflow = new WorkflowFake();
      workflow.Do(new TestActivity("Result 1"));
      workflow.Do(new TestActivity("Result 2"));
      workflow.Do(new TestActivity("Result 3"));

      WorkflowResult = new WorkflowRunner().Run(workflow);

      Assert.IsTrue(WorkflowResult.Success);
      Assert.AreEqual(CoreConstants.WORKFLOW_COMPLETED_SUCCESSFULLY, WorkflowResult.Summary);
      Assert.AreEqual(3, WorkflowResult.ActivityResults.Count);

      Assert.AreEqual("Result 1", WorkflowResult.ActivityResults[0].Summary);
      Assert.AreEqual("Result 2", WorkflowResult.ActivityResults[1].Summary);
      Assert.AreEqual("Result 3", WorkflowResult.ActivityResults[2].Summary);
    }

    [TestMethod]
    public void Run_EncountersErrorResult_ReturnsSuccessFalseResult()
    {
      IWorkflow workflow = new WorkflowFake();
      workflow.Do(new TestActivity("Result 1"));
      workflow.Do(new ErrorActivityFake("ERROR ENCOUNTERED!!"));
      workflow.Do(new TestActivity("Result 3"));

      WorkflowResult = new WorkflowRunner().Run(workflow);

      Assert.IsFalse(WorkflowResult.Success);
      Assert.AreEqual(string.Format(CoreConstants.WORKFLOW_FAILED, "ERROR ENCOUNTERED!!"), WorkflowResult.Summary);
      Assert.AreEqual(2, WorkflowResult.ActivityResults.Count);

      Assert.AreEqual("Result 1", WorkflowResult.ActivityResults[0].Summary);
      Assert.AreEqual("ERROR ENCOUNTERED!!", WorkflowResult.ActivityResults[1].Summary);
      Assert.IsInstanceOfType(WorkflowResult.ActivityResults[1], typeof(ErrorResult));
    }

    [TestMethod]
    public void Run_EncountersException_ReturnsSuccessFalseResult()
    {
      IWorkflow workflow = new WorkflowFake();
      workflow.Do(new TestActivity("Result 1"));
      workflow.Do(new ExceptionActivityFake(new InvalidOperationException("EXCEPTION ENCOUNTERED!!")));
      workflow.Do(new TestActivity("Result 3"));

      WorkflowResult = new WorkflowRunner().Run(workflow);

      Assert.IsFalse(WorkflowResult.Success);
      Assert.AreEqual(string.Format(CoreConstants.WORKFLOW_FAILED, "EXCEPTION ENCOUNTERED!!"), WorkflowResult.Summary);
      Assert.AreEqual(2, WorkflowResult.ActivityResults.Count);

      Assert.AreEqual("Result 1", WorkflowResult.ActivityResults[0].Summary);
      Assert.AreEqual("EXCEPTION ENCOUNTERED!!", WorkflowResult.ActivityResults[1].Summary);
      Assert.IsInstanceOfType(WorkflowResult.ActivityResults[1], typeof(ExceptionResult));

      var exResult = (ExceptionResult)WorkflowResult.ActivityResults[1];
      Assert.AreEqual(exResult.Summary, exResult.Exception.Message);
      Assert.IsInstanceOfType(exResult.Exception, typeof(InvalidOperationException));
    }

    [TestMethod]
    public void Run_EncountersNotify_ReturnsSuccessTrueResult()
    {
      IWorkflow workflow = new WorkflowFake();
      workflow.Do(new TestActivity("Result 1"));
      workflow.Do(new NotifyActivityFake(new InvalidOperationException("EXCEPTION ENCOUNTERED!!")));
      workflow.Do(new TestActivity("Result 3"));

      WorkflowResult = new WorkflowRunner().Run(workflow);

      Assert.IsTrue(WorkflowResult.Success);
      Assert.AreEqual(CoreConstants.WORKFLOW_COMPLETED_SUCCESSFULLY, WorkflowResult.Summary);
      Assert.AreEqual(3, WorkflowResult.ActivityResults.Count);

      Assert.AreEqual("Result 1", WorkflowResult.ActivityResults[0].Summary);
      Assert.AreEqual("EXCEPTION ENCOUNTERED!!", WorkflowResult.ActivityResults[1].Summary);
      Assert.IsInstanceOfType(WorkflowResult.ActivityResults[1], typeof(NotifyResult));
      Assert.AreEqual("Result 3", WorkflowResult.ActivityResults[2].Summary);

      var notifyResult = (NotifyResult)WorkflowResult.ActivityResults[1];
      Assert.AreEqual(notifyResult.Summary, notifyResult.Exception.Message);
      Assert.IsInstanceOfType(notifyResult.Exception, typeof(InvalidOperationException));
    }

    [TestMethod]
    public void Run_TransactionStartNoTransactionCommitted_AllActivitiesAfterStartInTransaction()
    {
      IWorkflow workflow = new WorkflowFake();
      workflow.Do(new TestActivity("Result 1"));
      workflow.TransactionStart();
      workflow.Do(new TestActivity("Result 2"));
      workflow.Do(new TestActivity("Result 3"));

      WorkflowResult = new WorkflowRunner().Run(workflow);

      Assert.IsTrue(WorkflowResult.Success);
      Assert.AreEqual(CoreConstants.WORKFLOW_COMPLETED_SUCCESSFULLY, WorkflowResult.Summary);
      Assert.AreEqual(5, WorkflowResult.ActivityResults.Count);

      Assert.AreEqual("Result 1", WorkflowResult.ActivityResults[0].Summary);
      Assert.AreEqual(CoreConstants.TRANSACTION_STARTED, WorkflowResult.ActivityResults[1].Summary);
      Assert.AreEqual("Result 2", WorkflowResult.ActivityResults[2].Summary);
      Assert.AreEqual("Result 3", WorkflowResult.ActivityResults[3].Summary);
      Assert.AreEqual(CoreConstants.TRANSACTION_COMMITED, WorkflowResult.ActivityResults[4].Summary);
    }

    [TestMethod]
    public void Run_TransactionStartAndTransactionCommitted_OnlyActivitiesBetweenStartAndCommitInTransaction()
    {
      IWorkflow workflow = new WorkflowFake();
      workflow.Do(new TestActivity("Result 1"));
      workflow.TransactionStart();
      workflow.Do(new TestActivity("Result 2"));
      workflow.TransactionCommit();
      workflow.Do(new TestActivity("Result 3"));

      WorkflowResult = new WorkflowRunner().Run(workflow);

      Assert.IsTrue(WorkflowResult.Success);
      Assert.AreEqual(CoreConstants.WORKFLOW_COMPLETED_SUCCESSFULLY, WorkflowResult.Summary);
      Assert.AreEqual(5, WorkflowResult.ActivityResults.Count);

      Assert.AreEqual("Result 1", WorkflowResult.ActivityResults[0].Summary);
      Assert.AreEqual(CoreConstants.TRANSACTION_STARTED, WorkflowResult.ActivityResults[1].Summary);
      Assert.AreEqual("Result 2", WorkflowResult.ActivityResults[2].Summary);
      Assert.AreEqual(CoreConstants.TRANSACTION_COMMITED, WorkflowResult.ActivityResults[3].Summary);
      Assert.AreEqual("Result 3", WorkflowResult.ActivityResults[4].Summary);
      
    }

    [TestMethod]
    public void Run_InTransaction_EncountersErrorResult_TransactionRolledBack()
    {
      IWorkflow workflow = new WorkflowFake();
      workflow.Do(new TestActivity("Result 1"));
      workflow.TransactionStart();
      workflow.Do(new TestActivity("Result 2"));
      workflow.Do(new ErrorActivityFake("ERROR ENCOUNTERED!!"));
      workflow.Do(new TestActivity("Result 3"));

      WorkflowResult = new WorkflowRunner().Run(workflow);

      Assert.IsFalse(WorkflowResult.Success);
      Assert.AreEqual(string.Format(CoreConstants.WORKFLOW_FAILED, "ERROR ENCOUNTERED!!"), WorkflowResult.Summary);
      Assert.AreEqual(5, WorkflowResult.ActivityResults.Count);

      Assert.AreEqual("Result 1", WorkflowResult.ActivityResults[0].Summary);
      Assert.AreEqual(CoreConstants.TRANSACTION_STARTED, WorkflowResult.ActivityResults[1].Summary);
      Assert.AreEqual("Result 2", WorkflowResult.ActivityResults[2].Summary);

      Assert.AreEqual("ERROR ENCOUNTERED!!", WorkflowResult.ActivityResults[3].Summary);
      Assert.IsInstanceOfType(WorkflowResult.ActivityResults[3], typeof(ErrorResult));

      Assert.AreEqual(CoreConstants.TRANSACTION_ROLLED_BACK, WorkflowResult.ActivityResults[4].Summary);
    }

    [TestMethod]
    public void Run_InTransaction_EncountersException_ReturnsSuccessFalseResult()
    {
      IWorkflow workflow = new WorkflowFake();
      workflow.Do(new TestActivity("Result 1"));
      workflow.TransactionStart();
      workflow.Do(new TestActivity("Result 2"));
      workflow.Do(new ExceptionActivityFake(new InvalidOperationException("EXCEPTION ENCOUNTERED!!")));
      workflow.Do(new TestActivity("Result 3"));

      WorkflowResult result = new WorkflowRunner().Run(workflow);

      Assert.IsFalse(result.Success);
      Assert.AreEqual(string.Format(CoreConstants.WORKFLOW_FAILED, "EXCEPTION ENCOUNTERED!!"), result.Summary);
      Assert.AreEqual(5, result.ActivityResults.Count);

      Assert.AreEqual("Result 1", result.ActivityResults[0].Summary);
      Assert.AreEqual(CoreConstants.TRANSACTION_STARTED, result.ActivityResults[1].Summary);
      Assert.AreEqual("Result 2", result.ActivityResults[2].Summary);
      Assert.AreEqual("EXCEPTION ENCOUNTERED!!", result.ActivityResults[3].Summary);
      Assert.IsInstanceOfType(result.ActivityResults[3], typeof(ExceptionResult));

      var exResult = (ExceptionResult)result.ActivityResults[3];
      Assert.AreEqual(exResult.Summary, exResult.Exception.Message);
      Assert.IsInstanceOfType(exResult.Exception, typeof(InvalidOperationException));

      Assert.AreEqual(CoreConstants.TRANSACTION_ROLLED_BACK, result.ActivityResults[4].Summary);
    }

    [TestMethod]
    public void Run_InTransaction_EncountersNotifyResult_TransactionCommitted()
    {
      IWorkflow workflow = new WorkflowFake();
      workflow.Do(new TestActivity("Result 1"));
      workflow.TransactionStart();
      workflow.Do(new TestActivity("Result 2"));
      workflow.Do(new NotifyActivityFake(new InvalidOperationException("EXCEPTION ENCOUNTERED!!")));
      workflow.Do(new TestActivity("Result 3"));

      WorkflowResult = new WorkflowRunner().Run(workflow);

      Assert.IsTrue(WorkflowResult.Success);
      Assert.AreEqual(CoreConstants.WORKFLOW_COMPLETED_SUCCESSFULLY, WorkflowResult.Summary);
      Assert.AreEqual(6, WorkflowResult.ActivityResults.Count);

      Assert.AreEqual("Result 1", WorkflowResult.ActivityResults[0].Summary);
      Assert.AreEqual(CoreConstants.TRANSACTION_STARTED, WorkflowResult.ActivityResults[1].Summary);
      Assert.AreEqual("Result 2", WorkflowResult.ActivityResults[2].Summary);
      Assert.AreEqual("EXCEPTION ENCOUNTERED!!", WorkflowResult.ActivityResults[3].Summary);
      Assert.IsInstanceOfType(WorkflowResult.ActivityResults[3], typeof(NotifyResult));
      Assert.AreEqual("Result 3", WorkflowResult.ActivityResults[4].Summary);
      Assert.AreEqual(CoreConstants.TRANSACTION_COMMITED, WorkflowResult.ActivityResults[5].Summary);
    }

    [TestMethod]
    public void TestBssTransactionTimeoutValue_ValidTimeoutValue()
    {
      ConfigurationManager.AppSettings["BSSTransactionTimeout"] = "00:10:00";
      WorkflowRunner.ResetTimeoutValue();
      Assert.AreEqual(TimeSpan.FromMinutes(10), WorkflowRunner.GetTransactionTimeoutValue());
    }

    [TestMethod]
    public void TestBssTransactionTimeoutValue_MissingTimeoutValue()
    {
      ConfigurationManager.AppSettings["BSSTransactionTimeout"] = null;
      WorkflowRunner.ResetTimeoutValue();
      Assert.AreEqual(TimeSpan.FromMinutes(5), WorkflowRunner.GetTransactionTimeoutValue());
    }

    [TestMethod]
    public void TestBssTransactionTimeoutValue_InvalidTimeoutValue()
    {
      ConfigurationManager.AppSettings["BSSTransactionTimeout"] = "0X:10:00";
      WorkflowRunner.ResetTimeoutValue();
      Assert.AreEqual(TimeSpan.FromMinutes(5), WorkflowRunner.GetTransactionTimeoutValue());
    }
  }

  internal class WorkflowFake : Workflow
  {
    public WorkflowFake() : base(new Inputs()) { }
  }

  internal class TestActivity : IActivity
  {
    private readonly string _summary;

    public TestActivity(string summary)
    {
      _summary = summary;
    }

    public ActivityResult Execute(Inputs inputs )
    {
      return new ActivityResult {Summary = _summary};
    }
  }

  internal class ErrorActivityFake : IActivity
  {
    private readonly string _summary;

    public ErrorActivityFake(string summary)
    {
      _summary = summary;
    }

    public ActivityResult Execute(Inputs inputs)
    {
      return new ErrorResult { Summary = _summary };
    }
  }
 
  internal class ExceptionActivityFake : IActivity
  {
    private readonly Exception _exception;

    public ExceptionActivityFake(Exception exception)
    {
      _exception = exception;
    }

    public ActivityResult Execute(Inputs inputs)
    {
      throw _exception;
    }
  }

  internal class NotifyActivityFake : IActivity
  {
    private readonly Exception _exception;

    public NotifyActivityFake(Exception exception)
    {
      _exception = exception;
    }

    public ActivityResult Execute(Inputs inputs)
    {
      return new NotifyResult {Exception = _exception};
    }
  }
}

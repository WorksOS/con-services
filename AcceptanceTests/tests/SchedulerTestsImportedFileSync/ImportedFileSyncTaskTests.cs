using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;

namespace SchedulerTestsImportedFileSync
{
  [TestClass]
  public class ImportedFileSyncTaskTests : TestControllerBase
  {
    private ILogger _log;
    private string ImportedProjectFileSyncSurveyedSurfaceTask = "ImportedProjectFileSyncSurveyedSurfaceTask";
    private string ImportedProjectFileSyncNonSurveyedSurfaceTask = "ImportedProjectFileSyncNonSurveyedSurfaceTask";

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFileSyncTaskTests>();
      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void ImportedFilesSyncSurvedSurfaceTaskExists()
    {
      var theJob = GetJob(HangfireConnection(), ImportedProjectFileSyncSurveyedSurfaceTask);
    
      Assert.IsNotNull(theJob,$"{ImportedProjectFileSyncSurveyedSurfaceTask} not found");
      Assert.AreEqual(ImportedProjectFileSyncSurveyedSurfaceTask, theJob.Id, "wrong job selected");
    }

    [TestMethod]
    public void ImportedFilesSyncNonSurvedSurfaceTaskExists()
    {
      var theJob = GetJob(HangfireConnection(), ImportedProjectFileSyncNonSurveyedSurfaceTask);

      Assert.IsNotNull(theJob, $"{ImportedProjectFileSyncNonSurveyedSurfaceTask} not found");
      Assert.AreEqual(ImportedProjectFileSyncNonSurveyedSurfaceTask, theJob.Id, "wrong job selected");
    }

    [TestMethod]
    public void ImportedProjectFileSyncSurveyedSurfaceTaskNextRuntime()
    {
      var theJob = GetJob(HangfireConnection(), ImportedProjectFileSyncSurveyedSurfaceTask);

      Assert.IsNotNull(theJob, $"{ImportedProjectFileSyncSurveyedSurfaceTask} not found");
      Assert.AreEqual(ImportedProjectFileSyncSurveyedSurfaceTask, theJob.Id, "wrong job selected");
      Assert.IsNotNull(theJob.NextExecution, $"{ImportedProjectFileSyncSurveyedSurfaceTask} nextExecutionTime not found");
      Assert.IsTrue(theJob.NextExecution < DateTime.UtcNow.AddMinutes(1).AddSeconds(1), $"{ImportedProjectFileSyncSurveyedSurfaceTask} nextExecutionTime not within one minute");
    }

    [TestMethod]
    public void ImportedProjectFileSyncNonSurveyedSurfaceTaskNextRuntime()
    {
      var theJob = GetJob(HangfireConnection(), ImportedProjectFileSyncNonSurveyedSurfaceTask);

      Assert.IsNotNull(theJob, $"{ImportedProjectFileSyncNonSurveyedSurfaceTask} not found");
      Assert.AreEqual(ImportedProjectFileSyncNonSurveyedSurfaceTask, theJob.Id, "wrong job selected");
      Assert.IsNotNull(theJob.NextExecution, $"{ImportedProjectFileSyncNonSurveyedSurfaceTask} nextExecutionTime not found");
      Assert.IsTrue(theJob.NextExecution < DateTime.UtcNow.AddMinutes(1).AddSeconds(1), $"{ImportedProjectFileSyncNonSurveyedSurfaceTask} nextExecutionTime not within one minute");
    }
  }
}

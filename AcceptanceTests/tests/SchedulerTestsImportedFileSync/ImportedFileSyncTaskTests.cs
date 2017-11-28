using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;

namespace SchedulerTestsImportedFileSync
{
  [TestClass]
  public class ImportedFileSyncTaskTests : TestControllerBase
  {
    private ILogger _log;
    private string ImportedProjectFileSyncTask = "ImportedProjectFileSyncTask";

    [TestInitialize]
    public void Init()
    {
      SetupDi();

      _log = LoggerFactory.CreateLogger<ImportedFileSyncTaskTests>();
      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void ImportedFilesSyncTaskExists()
    {
      var theJob = GetJob(HangfireConnection(), ImportedProjectFileSyncTask);
    
      Assert.IsNotNull(theJob,$"{ImportedProjectFileSyncTask} not found");
      Assert.AreEqual(ImportedProjectFileSyncTask, theJob.Id, "wrong job selected");
    }

    [TestMethod]
    public void ImportedProjectFileSyncTaskNextRuntime()
    {
      var theJob = GetJob(HangfireConnection(), ImportedProjectFileSyncTask);

      Assert.IsNotNull(theJob, $"{ImportedProjectFileSyncTask} not found");
      Assert.AreEqual(ImportedProjectFileSyncTask, theJob.Id, "wrong job selected");
      Assert.IsNotNull(theJob.NextExecution, $"{ImportedProjectFileSyncTask} nextExecutionTime not found");
      Assert.IsTrue(theJob.NextExecution < DateTime.UtcNow.AddMinutes(1).AddSeconds(1), $"{ImportedProjectFileSyncTask} nextExecutionTime not within one minute");
    }
  }
}

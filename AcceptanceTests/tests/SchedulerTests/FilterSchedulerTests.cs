using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Dapper;
using MySql.Data.MySqlClient;
using VSS.Productivity3D.Scheduler.Common.Utilities;

//using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace SchedulerTests
{
  [TestClass]
  public class FilterSchedulerTests : TestControllerBase
  {
    protected ILogger log;
    string filterCleanupJob = "FilterCleanupJob";

    [TestInitialize]
    public void Init()
    {
      SetupDI();

      log = loggerFactory.CreateLogger<FilterSchedulerTests>();
      Assert.IsNotNull(log, "log is null");
    }

    [TestMethod]
    public void FilterScheduleTaskExists()
    {
      var theJob = GetJob(HangfireConnection(), filterCleanupJob);
    
      Assert.IsNotNull(theJob,$"{filterCleanupJob} not found");
      Assert.AreEqual(filterCleanupJob, theJob.Id, "wrong job selected");
    }

    [TestMethod]
    public void FilterScheduleTaskNextRuntime()
    {
      var theJob = GetJob(HangfireConnection(), filterCleanupJob);

      Assert.IsNotNull(theJob, $"{filterCleanupJob} not found");
      Assert.AreEqual(filterCleanupJob, theJob.Id, "wrong job selected");
      Assert.IsNotNull(theJob.NextExecution, $"{filterCleanupJob} nextExecutionTime not found");
      Assert.IsTrue(theJob.NextExecution < DateTime.UtcNow.AddMinutes(1).AddSeconds(1), $"{filterCleanupJob} nextExecutionTime not within one minute");
    }

    [TestMethod]
    public void FilterScheduleTask_WaitForCleanup()
    {
      var theJob = GetJob(HangfireConnection(), filterCleanupJob);

      string filterDbConnectionString = ConnectionUtils.GetConnectionString(configStore, log, "_FILTER");
      var dbConnection = new MySqlConnection(filterDbConnectionString);
      dbConnection.Open();

      //var filter = new Filter()
      //{};
      var filterUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var userUid = Guid.NewGuid().ToString();
      var name = "";
      var filterJson = "";
      var actionUtc = new DateTime(2017, 1, 1); // eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff
      var empty = "\"";
      string insertFilter = string.Format(
        $"INSERT Filter (FilterUID, fk_CustomerUid, fk_ProjectUID, UserID, Name, FilterJson, LastActionedUTC) " +
        $"VALUES ({empty}{filterUid}{empty}, { empty}{customerUid}{empty}, {empty}{projectUid}{empty}, {empty}{userUid}{empty}, " +
        $"{empty}{filterJson}{empty}, {empty}{name}{empty}, {empty}{actionUtc.ToString($"yyyy-MM-dd HH:mm:ss.fffffff")}{empty})");

      int insertedCount = 0;
      insertedCount = dbConnection.Execute(insertFilter);
      Assert.AreEqual(1,insertedCount,"Filter Not Inserted");

      Debug.Assert(theJob.NextExecution != null, "theJob.NextExecution != null");
      var msToWait = (int)(theJob.NextExecution.Value - DateTime.UtcNow).TotalMilliseconds + 50;
      if (msToWait > 0 )
        Thread.Sleep(msToWait);

      string selectFilter = string.Format($"SELECT FilterUID FROM Filter WHERE FilterUID = {empty}{filterUid}{empty}");
      var retrievedFilter = dbConnection.Query(selectFilter);

      Assert.IsNull(retrievedFilter, "Filter should no longer exist.");
      dbConnection.Close();
    }
  }
}

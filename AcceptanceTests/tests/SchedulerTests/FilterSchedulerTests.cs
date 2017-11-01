using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace SchedulerTests
{
  [TestClass]
  public class FilterSchedulerTests : TestControllerBase
  {
    protected ILogger _log;
    string FilterCleanupTask = "FilterCleanupTask";
    private int _bufferForDBUpdateMs = 1000;

    [TestInitialize]
    public void Init()
    {
      SetupDI();

      _log = loggerFactory.CreateLogger<FilterSchedulerTests>();
      Assert.IsNotNull(_log, "log is null");
    }

    [TestMethod]
    public void FilterScheduleTaskExists()
    {
      var theJob = GetJob(HangfireConnection(), FilterCleanupTask);
    
      Assert.IsNotNull(theJob,$"{FilterCleanupTask} not found");
      Assert.AreEqual(FilterCleanupTask, theJob.Id, "wrong job selected");
    }

    [TestMethod]
    public void FilterScheduleTaskNextRuntime()
    {
      var theJob = GetJob(HangfireConnection(), FilterCleanupTask);

      Assert.IsNotNull(theJob, $"{FilterCleanupTask} not found");
      Assert.AreEqual(FilterCleanupTask, theJob.Id, "wrong job selected");
      Assert.IsNotNull(theJob.NextExecution, $"{FilterCleanupTask} nextExecutionTime not found");
      Assert.IsTrue(theJob.NextExecution < DateTime.UtcNow.AddMinutes(1).AddSeconds(1), $"{FilterCleanupTask} nextExecutionTime not within one minute");
    }

    [TestMethod]
    public void FilterScheduleTask_WaitForCleanup()
    {
      var theJob = GetJob(HangfireConnection(), FilterCleanupTask);

      string filterDbConnectionString = ConnectionUtils.GetConnectionString(configStore, _log, "_FILTER");
      var dbConnection = new MySqlConnection(filterDbConnectionString);
      dbConnection.Open();

      //var filter = new Filter(){}; //todo?
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

      // todo is the NextExecution datetime not accurate e.g. actually done on some boundary?
      var nextExec = theJob.NextExecution.Value;
      var nowUTC = DateTime.UtcNow;
      var msToWait = (int)(nextExec - nowUTC).TotalMilliseconds + _bufferForDBUpdateMs;
      if (msToWait > 0 )
        Thread.Sleep(msToWait);
      
      string selectFilter = string.Format($"SELECT FilterUID FROM Filter WHERE FilterUID = {empty}{filterUid}{empty}");
      var response = dbConnection.Query(selectFilter);
      Console.WriteLine($"FilterScheduleTask_WaitForCleanup: nextExec {nextExec} nowUTC {nowUTC} _bufferForDBUpdateMs {_bufferForDBUpdateMs} msToWait {msToWait}  insertFilter {insertFilter} selectFilter {selectFilter} response {JsonConvert.SerializeObject(response)} connectionString {dbConnection.ConnectionString}");

      Assert.IsNotNull(response, "Should have a response.");
      Assert.AreEqual(0, response.Count(), "No filters should be returned.");

      dbConnection.Close();
    }
  }
}

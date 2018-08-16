using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RepositoryTests
{
  [TestClass]
  public class SchedulerRepositoryTests : TestControllerBase
  {
    [TestInitialize]
    public void Init()
    {
      SetupDI();
    }


    [TestMethod]
    public void SchedulerSchemaExists_JobTable()
    {
      const string tableName = "Job";
      List<string> columnNames = new List<string>
      {
        "Id",
        "StateId",
        "StateName",
        "InvocationData",
        "Arguments",
        "CreatedAt",
        "ExpireAt"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_CounterTable()
    {
      const string tableName = "Counter";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Key",
        "Value",
        "ExpireAt"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_AggregatedCounterTable()
    {
      const string tableName = "AggregatedCounter";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Key",
        "Value",
        "ExpireAt"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_DistributedLockTable()
    {
      const string tableName = "DistributedLock";
      List<string> columnNames = new List<string>
      {
        "Resource",
        "CreatedAt"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_HashTable()
    {
      const string tableName = "Hash";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Key",
        "Field",
        "Value",
        "ExpireAt"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_JobParameterTable()
    {
      const string tableName = "JobParameter";
      List<string> columnNames = new List<string>
      {
        "Id",
        "JobId",
        "Name",
        "Value"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_JobQueueTable()
    {
      const string tableName = "JobQueue";
      List<string> columnNames = new List<string>
      {
        "Id",
        "JobId",
        "Queue",
        "FetchedAt",
        "FetchToken"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_JobStateTable()
    {
      const string tableName = "JobState";
      List<string> columnNames = new List<string>
      {
        "Id",
        "JobId",
        "Name",
        "Reason",
        "CreatedAt",
        "Data"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_ServerQueueTable()
    {
      const string tableName = "Server";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Data",
        "LastHeartbeat"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_SetQueueTable()
    {
      const string tableName = "Set";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Key",
        "Value",
        "Score",
        "ExpireAt"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_StateQueueTable()
    {
      const string tableName = "State";
      List<string> columnNames = new List<string>
      {
        "Id",
        "JobId",
        "Name",
        "Reason",
        "CreatedAt",
        "Data"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_ListQueueTable()
    {
      const string tableName = "List";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Key",
        "Value",
        "ExpireAt"
      };
      CheckSchema(tableName, columnNames);
    }
  }
}

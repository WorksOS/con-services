using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests
{
  [TestClass]
  public class RepositoryTests : TestControllerBase
  {

    [TestInitialize]
    public void Init()
    {
      SetupLogging();
    }

    [TestMethod]
    public void FilterSchemaExists()
    {
      const string tableName = "Filter";
      List<string> columnNames = new List<string>
      {
        "ID",
        "FilterUID",
        "fk_CustomerUID",
        "fk_ProjectUID",
        "fk_UserUID",
        "Name",
        "FilterJson",
        "IsDeleted",
        "LastActionedUTC",
        "InsertUTC",
        "UpdateUTC"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists()
    {
      const string tableName = "Job";
      List<string> columnNames = new List<string>
      {
        "ID",
        "FilterUID",
        "fk_CustomerUID",
        "fk_ProjectUID",
        "fk_UserUID",
        "Name",
        "FilterJson",
        "IsDeleted",
        "LastActionedUTC",
        "InsertUTC",
        "UpdateUTC"
      };
      CheckSchema(tableName, columnNames);
    }

  }
}

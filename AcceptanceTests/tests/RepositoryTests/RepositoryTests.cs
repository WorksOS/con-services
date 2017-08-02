using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
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
        "ID", "FilterUID", "fk_CustomerUID", "fk_ProjectUID", "fk_UserUID" , "Name" , "FilterJson", "IsDeleted", "LastActionedUTC", "InsertUTC", "UpdateUTC"
      };
      CheckSchema(tableName, columnNames);
    }

    /// <summary>
    /// Create Happy path i.e. filter doesn't exist
    /// </summary>
    [TestMethod]
    public void CreateTransientFilter_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        FilterUID = Guid.NewGuid(),
        Name = "",
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserUid = createFilterEvent.UserUID.ToString(),
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = createFilterEvent.Name,
        FilterJson = createFilterEvent.FilterJson,
        LastActionedUtc = createFilterEvent.ActionUTC
      };

      var s = filterRepo.StoreEvent(createFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event not written");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");

      var f = filterRepo.GetFiltersForProject(createFilterEvent.ProjectUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filters from filterRepo");
      Assert.AreEqual(1, f.Result.Count(), "retrieved filter count is incorrect");
      Assert.AreEqual(filter, f.Result.AsList()[0], "retrieved filter is incorrect");

      f = filterRepo.GetFiltersForProjectUser(createFilterEvent.CustomerUID.ToString(), createFilterEvent.ProjectUID.ToString(), createFilterEvent.UserUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve user filters from filterRepo");
      Assert.AreEqual(1, f.Result.Count(), "retrieved user filter count is incorrect");
      Assert.AreEqual(filter, f.Result.AsList()[0], "retrieved user filter is incorrect");
    }

    /// <summary>
    /// Update Happy path i.e. filter exists
    /// </summary>
    [TestMethod]
    public void UpdateTransientFilter_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        FilterUID = Guid.NewGuid(),
        Name = "",
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent()
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserUID = createFilterEvent.UserUID,
        FilterUID = createFilterEvent.FilterUID,
        Name = "",
        FilterJson = "blahDeBlah",
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserUid = createFilterEvent.UserUID.ToString(),
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = createFilterEvent.Name,
        FilterJson = updateFilterEvent.FilterJson,
        LastActionedUtc = updateFilterEvent.ActionUTC
      };

      filterRepo.StoreEvent(createFilterEvent).Wait();

      var s = filterRepo.StoreEvent(updateFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event not updated");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");
    }

    /// <summary>
    /// Delete Happy path i.e. filter exists
    /// </summary>
    [TestMethod]
    public void DeleteTransientFilter_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        FilterUID = Guid.NewGuid(),
        Name = "",
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var deleteFilterEvent = new DeleteFilterEvent()
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserUID = createFilterEvent.UserUID,
        FilterUID = createFilterEvent.FilterUID,
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      filterRepo.StoreEvent(createFilterEvent).Wait();

      var s = filterRepo.StoreEvent(deleteFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event not deleted");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve filter from filterRepo");
    }

    /// <summary>
    /// Update Persistant unHappy path i.e. not allowed to update persistant
    /// </summary>
    [TestMethod]
    public void UpdatePersistantFilter_UnhappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        FilterUID = Guid.NewGuid(),
        Name = "persistant",
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent()
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserUID = createFilterEvent.UserUID,
        FilterUID = createFilterEvent.FilterUID,
        Name = "changed",
        FilterJson = "blahDeBlah",
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserUid = createFilterEvent.UserUID.ToString(),
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = createFilterEvent.Name,
        FilterJson = createFilterEvent.FilterJson,
        LastActionedUtc = createFilterEvent.ActionUTC
      };

      filterRepo.StoreEvent(createFilterEvent).Wait();

      var s = filterRepo.StoreEvent(updateFilterEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Filter event should not be updated");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");
    }

    /// <summary>
    /// Update Persistant unHappy path i.e. update received before create
    /// </summary>
    [TestMethod]
    public void UpdatePersistantFilter_OutOfOrder()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        FilterUID = Guid.NewGuid(),
        Name = "persistant",
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent()
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserUID = createFilterEvent.UserUID,
        FilterUID = createFilterEvent.FilterUID,
        Name = "changed",
        FilterJson = "blahDeBlah",
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserUid = createFilterEvent.UserUID.ToString(),
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = updateFilterEvent.Name,
        FilterJson = updateFilterEvent.FilterJson,
        LastActionedUtc = updateFilterEvent.ActionUTC
      };

      var s = filterRepo.StoreEvent(updateFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event should be created with the update");

      s = filterRepo.StoreEvent(createFilterEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Filter event should not be updated with the create");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");
    }

    /// <summary>
    /// Delete unHappy path i.e. filter doesn't exist
    ///   actually a deleted filter is created in case of out-of-order i.e. delete received before create.
    ///   this ensures that the filters 'deleted' status is retained.
    /// </summary>
    [TestMethod]
    public void DeleteTransientFilter_UnhappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var deleteFilterEvent = new DeleteFilterEvent()
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        FilterUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var s = filterRepo.StoreEvent(deleteFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event set to deleted");

      var g = filterRepo.GetFilter(deleteFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve deleted filter from filterRepo");

      g = filterRepo.GetFilterForUnitTest(deleteFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Should be able to retrieve deleted filter from filterRepo");
    }

    #region privates

    private void CheckSchema(string tableName, List<string> columnNames)
    {
      using (var connection = new MySqlConnection(configStore.GetConnectionString("VSPDB")))
      {
        try
        {
          connection.Open();

          //Check table exists
          var table = connection.Query(GetQuery(tableName, true)).FirstOrDefault();
          Assert.IsNotNull(table, "Missing " + tableName + " table schema");
          Assert.AreEqual(tableName, table.TABLE_NAME, "Wrong table name");

          //Check table columns exist
          var columns = connection.Query(GetQuery(tableName, false)).ToList();
          Assert.IsNotNull(columns, "Missing " + tableName + " table columns");
          Assert.AreEqual(columnNames.Count, columns.Count, "Wrong number of " + tableName + " columns");
          foreach (var columnName in columnNames)
            Assert.IsNotNull(columns.Find(c => c.COLUMN_NAME == columnName), "Missing " + columnName + " column in " + tableName + " table");
        }
        finally
        {
          connection.Close();
        }
      }
    }

    private string GetQuery(string tableName, bool selectTable)
    {
      string what = selectTable ? "TABLE_NAME" : "COLUMN_NAME";
      var query = string.Format("SELECT {0} FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{1}' AND TABLE_NAME ='{2}'",
        what, configStore.GetValueString("MYSQL_DATABASE_NAME"), tableName);
      return query;
    }

    #endregion privates
  }
}

using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
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
      SetupLoggingAndRepos();
    }

    #region Filters
    [TestMethod]
    public void FilterSchemaExists()
    {
      const string tableName = "Filter";
      List<string> columnNames = new List<string>
      {
        "ID", "FilterUID", "fk_CustomerUID", "fk_ProjectUID", "UserID" , "Name" , "fk_FilterTypeID", "FilterJson", "IsDeleted", "LastActionedUTC", "InsertUTC", "UpdateUTC"
      };

      CheckSchema(tableName, columnNames);
    }

    /// <summary>
    /// Get Happy path i.e. active, persistent only
    /// </summary>
    [TestMethod]
    public void GetFiltersForProject_PersistentOnly()
    {
      var custUid = Guid.Parse("f57e30d2-ad64-452a-8d6c-0afd790eab6c");

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createTransientFilterEvent1 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterType = FilterType.Transient,
        FilterJson = "blah1",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var createTransientFilterEvent2 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = "Transient 2",
        FilterType = FilterType.Transient,
        FilterJson = "blah2",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createPersistentFilterEvent1 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.Parse("6396c4b0-3ed4-40fa-afed-035457252463"),
        Name = "dateRangeType=Today with polygonLL",
        FilterType = FilterType.Persistent,
        FilterJson = "{\"startUtc\":null,\"endUtc\":null,\"dateRangeType\":0,\"designUID\":null,\"contributingMachines\":null,\"onMachineDesignID\":null,\"vibeStateOn\":null,\"polygonUID\":\"ca9c91c3-513b-4082-b2d7-0568899e56d5\",\"polygonName\":null,\"polygonLL\":[{\"Lat\":36.207118,\"Lon\":-115.01848},{\"Lat\":36.207334,\"Lon\":-115.018394},{\"Lat\":36.207492,\"Lon\":-115.019604},{\"Lat\":36.207101,\"Lon\":-115.019478}],\"forwardDirection\":null,\"layerNumber\":null}",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createPersistentFilterEvent2 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.Parse("59e28218-0164-417b-8de0-9ed8f96d6ed2"),
        Name = "dateRangeType=Yesterday with polygonLL",
        FilterType = FilterType.Persistent,
        FilterJson = "{\"startUtc\":null,\"endUtc\":null,\"dateRangeType\":1,\"designUID\":null,\"contributingMachines\":null,\"onMachineDesignID\":null,\"vibeStateOn\":null,\"polygonUID\":\"ca9c91c3-513b-4082-2d7-0568899e56d5\",\"polygonName\":null,\"polygonLL\":[{\"Lat\":36.207118,\"Lon\":-115.01848},{\"Lat\":36.207334,\"Lon\":-115.018394},{\"Lat\":36.207492,\"Lon\":-115.019604},{\"Lat\":36.207101,\"Lon\":-115.019478}],\"forwardDirection\":null,\"layerNumber\":null}",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createReportFilterEvent1 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = "dateRangeType=Today with polygonLL",//match a peristent filter name on purpose
        FilterType = FilterType.Report,
        FilterJson = "{\"startUtc\":null,\"endUtc\":null,\"dateRangeType\":0,\"designUID\":null,\"contributingMachines\":null,\"onMachineDesignID\":null,\"vibeStateOn\":null,\"polygonUID\":\"ca9c91c3-513b-4082-b2d7-0568899e56d5\",\"polygonName\":null,\"polygonLL\":[{\"Lat\":36.207118,\"Lon\":-115.01848},{\"Lat\":36.207334,\"Lon\":-115.018394},{\"Lat\":36.207492,\"Lon\":-115.019604},{\"Lat\":36.207101,\"Lon\":-115.019478}],\"forwardDirection\":null,\"layerNumber\":null}",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createReportFilterEvent2 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = "Report 2",
        FilterType = FilterType.Report,
        FilterJson = "{\"startUtc\":null,\"endUtc\":null,\"dateRangeType\":1,\"designUID\":null,\"contributingMachines\":null,\"onMachineDesignID\":null,\"vibeStateOn\":null,\"polygonUID\":\"ca9c91c3-513b-4082-2d7-0568899e56d5\",\"polygonName\":null,\"polygonLL\":[{\"Lat\":36.207118,\"Lon\":-115.01848},{\"Lat\":36.207334,\"Lon\":-115.018394},{\"Lat\":36.207492,\"Lon\":-115.019604},{\"Lat\":36.207101,\"Lon\":-115.019478}],\"forwardDirection\":null,\"layerNumber\":null}",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      this.FilterRepo.StoreEvent(createTransientFilterEvent1).Wait();
      this.FilterRepo.StoreEvent(createTransientFilterEvent2).Wait();
      this.FilterRepo.StoreEvent(createPersistentFilterEvent1).Wait();
      this.FilterRepo.StoreEvent(createPersistentFilterEvent2).Wait();
      this.FilterRepo.StoreEvent(createReportFilterEvent1).Wait();
      this.FilterRepo.StoreEvent(createReportFilterEvent2).Wait();

      var g = this.FilterRepo.GetFiltersForProject(TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filters from filterRepo");
      Assert.IsTrue(g.Result.Count() > 1, "retrieved filter count is incorrect");

      g = this.FilterRepo.GetFiltersForProjectUser(custUid.ToString(), TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID.ToString(), TestUtility.UIDs.JWT_USER_ID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filters from filterRepo");
      Assert.AreEqual(2, g.Result.Count(), "retrieved filter count is incorrect");

      var f = this.FilterRepo.GetFilter(createTransientFilterEvent1.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createTransientFilterEvent1.FilterUID.ToString(), f.Result.FilterUid, "retrieved filter UId is incorrect");

      f = this.FilterRepo.GetFilter(createTransientFilterEvent2.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createTransientFilterEvent2.FilterUID.ToString(), f.Result.FilterUid, "retrieved filter UId is incorrect");

      f = this.FilterRepo.GetFilter(createPersistentFilterEvent1.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createPersistentFilterEvent1.FilterUID.ToString(), f.Result.FilterUid, "retrieved filter UId is incorrect");

      f = this.FilterRepo.GetFilter(createPersistentFilterEvent2.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createPersistentFilterEvent2.FilterUID.ToString(), f.Result.FilterUid, "retrieved filter UId is incorrect");

      f = this.FilterRepo.GetFilter(createReportFilterEvent1.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createReportFilterEvent1.FilterUID.ToString(), f.Result.FilterUid, "retrieved filter UId is incorrect");

      f = this.FilterRepo.GetFilter(createReportFilterEvent2.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createReportFilterEvent2.FilterUID.ToString(), f.Result.FilterUid, "retrieved filter UId is incorrect");
    }

    /// <summary>
    /// Create Happy path i.e. filter doesn't exist
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow("SomeFilter")]
    public void CreateTransientFilter_HappyPath(string filterName)
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        ProjectUID = Guid.NewGuid(),
        FilterUID = Guid.NewGuid(),
        Name = filterName,
        FilterType = FilterType.Transient,
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserId = createFilterEvent.UserID,
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = createFilterEvent.Name,
        FilterType = createFilterEvent.FilterType,
        FilterJson = createFilterEvent.FilterJson,
        LastActionedUtc = createFilterEvent.ActionUTC
      };

      WriteEventToDb(createFilterEvent);

      var g = this.FilterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");

      var f = this.FilterRepo.GetFiltersForProject(createFilterEvent.ProjectUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filters from filterRepo");
      Assert.AreEqual(0, f.Result.Count(), "retrieved filter count is incorrect");

      f = this.FilterRepo.GetFiltersForProjectUser(createFilterEvent.CustomerUID.ToString(), createFilterEvent.ProjectUID.ToString(), createFilterEvent.UserID);
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve user filters from filterRepo");
      Assert.AreEqual(0, f.Result.Count(), "retrieved user filter count is incorrect");
    }

    /// <summary>
    /// Create Transient should allow duplicate blank Names
    /// </summary>
    [TestMethod]
    public void CreateTransientFilter_DuplicateBlankNamesAreValid()
    {
      var custUid = Guid.NewGuid();
      var projUid = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createTransientFilterEvent1 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterType = FilterType.Transient,
        FilterJson = "blah1",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createTransientFilterEvent2 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterType = FilterType.Transient,
        FilterJson = "blah2",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      WriteEventToDb(createTransientFilterEvent1);
      WriteEventToDb(createTransientFilterEvent2);
    }

    /// <summary>
    /// Create Persistent should not occur for duplicate Name.
    /// However, this is a business rule which should  be handled in the executor.
    /// </summary>
    [TestMethod]
    public void CreatePeristantFilter_DuplicateNamesNotValid()
    {
      var custUid = Guid.NewGuid();
      var projUid = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createPersistentFilterEvent1 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = "HasAName",
        FilterType = FilterType.Persistent,
        FilterJson = "blah1",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createPersistentFilterEvent2 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = "HasAName",
        FilterType = FilterType.Persistent,
        FilterJson = "blah2",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      WriteEventToDb(createPersistentFilterEvent1);
      WriteEventToDb(createPersistentFilterEvent2);
    }


    /// <summary>
    /// Create Persistent should not occur for duplicate Name.
    /// However, this is a business rule which should  be handled in the executor.
    /// </summary>
    [TestMethod]
    public void CreateReportFilter_DuplicateNamesAreValid()
    {
      var custUid = Guid.NewGuid();
      var projUid = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createReportFilterEvent1 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = "HasAName",
        FilterType = FilterType.Report,
        FilterJson = "blah1",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createReportFilterEvent2 = new CreateFilterEvent
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = "HasAName",
        FilterType = FilterType.Report,
        FilterJson = "blah2",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      WriteEventToDb(createReportFilterEvent1);
      WriteEventToDb(createReportFilterEvent2);
    }


    /// <summary>
    /// Update Happy path i.e. filter exists
    /// </summary>
    [TestMethod]
    public void UpdateTransientFilter_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterType = FilterType.Transient,
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        Name = string.Empty,
        FilterType = FilterType.Report,
        FilterJson = "blahDeBlah",
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserId = createFilterEvent.UserID,
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = createFilterEvent.Name,
        FilterType = createFilterEvent.FilterType,
        FilterJson = createFilterEvent.FilterJson,
        LastActionedUtc = createFilterEvent.ActionUTC
      };

      this.FilterRepo.StoreEvent(createFilterEvent).Wait();
      WriteEventToDb(updateFilterEvent, "Filter event not updateable", 0);

      var g = this.FilterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");
    }

    /// <summary>
    /// Update Transient unHappy path i.e. not allowed to update transient
    /// </summary>
    [TestMethod]
    public void UpdateTransientFilter_UnhappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = TestUtility.UIDs.JWT_USER_ID,
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterType = FilterType.Transient,
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        Name = string.Empty,
        FilterType = FilterType.Transient,
        FilterJson = "blahDeBlah",
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserId = createFilterEvent.UserID,
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = createFilterEvent.Name,
        FilterType = createFilterEvent.FilterType,
        FilterJson = createFilterEvent.FilterJson,
        LastActionedUtc = createFilterEvent.ActionUTC
      };

      this.FilterRepo.StoreEvent(createFilterEvent).Wait();

      WriteEventToDb(updateFilterEvent, "Filter event should not be updated", 0);

      var g = this.FilterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");
    }

    /// <summary>
    /// Update Persistent Happy path i.e. allowed to update persistent
    /// </summary>
    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void UpdatePersistentFilter_HappyPath(FilterType filterType)
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "persistent",
        FilterType = filterType,
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        Name = "changed",
        FilterJson = "blahDeBlah",
        FilterType = filterType,
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = updateFilterEvent.CustomerUID.ToString(),
        ProjectUid = updateFilterEvent.ProjectUID.ToString(),
        UserId = updateFilterEvent.UserID,
        FilterUid = updateFilterEvent.FilterUID.ToString(),
        Name = updateFilterEvent.Name,
        FilterType = updateFilterEvent.FilterType,
        FilterJson = updateFilterEvent.FilterJson,
        LastActionedUtc = updateFilterEvent.ActionUTC
      };

      this.FilterRepo.StoreEvent(createFilterEvent).Wait();
      WriteEventToDb(updateFilterEvent);

      var g = this.FilterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");
    }

    /// <summary>
    /// Update Persistent unHappy path i.e. update received before create
    /// </summary>
    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void UpdatePersistentFilter_OutOfOrder(FilterType filterType)
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "persistent",
        FilterType = filterType,
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        Name = "changed",
        FilterType = filterType,
        FilterJson = "blahDeBlah",
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      var filter = new Filter
      {
        CustomerUid = createFilterEvent.CustomerUID.ToString(),
        ProjectUid = createFilterEvent.ProjectUID.ToString(),
        UserId = createFilterEvent.UserID,
        FilterUid = createFilterEvent.FilterUID.ToString(),
        Name = updateFilterEvent.Name,
        FilterType = createFilterEvent.FilterType,
        FilterJson = updateFilterEvent.FilterJson,
        LastActionedUtc = updateFilterEvent.ActionUTC
      };

      WriteEventToDb(updateFilterEvent, "Filter event should be created with the update");
      WriteEventToDb(createFilterEvent, "Filter event should not be updated with the create", 0);

      var g = this.FilterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
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
      var createFilterEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        Name = string.Empty,
        FilterType = FilterType.Transient,
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var deleteFilterEvent = new DeleteFilterEvent
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      this.FilterRepo.StoreEvent(createFilterEvent).Wait();

      WriteEventToDb(deleteFilterEvent, "Filter event not deleted");

      var g = this.FilterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve filter from filterRepo");
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

      var deleteFilterEvent = new DeleteFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      WriteEventToDb(deleteFilterEvent, "Filter event set to deleted");

      var g = this.FilterRepo.GetFilter(deleteFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve deleted filter from filterRepo");

      g = this.FilterRepo.GetFilterForUnitTest(deleteFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Should be able to retrieve deleted filter from filterRepo");
    }

    /// <summary>
    /// Delete Happy path i.e. filter exists
    /// </summary>
    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public void DeletePersistentFilter_HappyPath(FilterType filterType)
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createFilterEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        ProjectUID = Guid.NewGuid(),
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "hasOne",
        FilterType = filterType,
        FilterJson = "blah",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var deleteFilterEvent = new DeleteFilterEvent
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        ActionUTC = firstCreatedUtc.AddMinutes(2),
        ReceivedUTC = firstCreatedUtc
      };

      this.FilterRepo.StoreEvent(createFilterEvent).Wait();

      WriteEventToDb(deleteFilterEvent, "Filter event not deleted");

      var g = this.FilterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve filter from filterRepo");
    }
    #endregion

    #region Boundaries
    [TestMethod]
    public void GeofenceSchemaExists()
    {
      const string tableName = "Geofence";
      List<string> columnNames = new List<string>
      {
        "ID", "GeofenceUID", "Name" , "fk_GeofenceTypeID", "GeometryWKT", "FillColor", "IsTransparent", "IsDeleted", "Description", "AreaSqMeters", "fk_CustomerUID", "UserUID", "LastActionedUTC", "InsertUTC", "UpdateUTC"
      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void ProjectGeofenceSchemaExists()
    {
      const string tableName = "ProjectGeofence";
      List<string> columnNames = new List<string>
      {
        "ID", "fk_GeofenceUID", "fk_ProjectUID" , "LastActionedUTC", "InsertUTC", "UpdateUTC"

      };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void GetAssociatedProjectGeofences()
    {
      var projUid = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createAssociateEvent1 = new AssociateProjectGeofence
      {
        ProjectUID = projUid,
        GeofenceUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var createAssociateEvent2 = new AssociateProjectGeofence
      {
        ProjectUID = projUid,
        GeofenceUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      this.ProjectRepo.StoreEvent(createAssociateEvent1).Wait();
      this.ProjectRepo.StoreEvent(createAssociateEvent2).Wait();
      var a = this.ProjectRepo.GetAssociatedGeofences(projUid.ToString());
      a.Wait();
      Assert.IsNotNull(a.Result, "Failed to get associated boundaries");
      Assert.AreEqual(2, a.Result.Count(), "Wrong number of boundaries retrieved");
    }

    [TestMethod]
    public void CreateAssociatedProjectGeofence_HappyPath()
    {
      var projUid = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createAssociateEvent = new AssociateProjectGeofence
      {
        ProjectUID = projUid,
        GeofenceUID = Guid.NewGuid(),
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      WriteEventToDb(createAssociateEvent, "Associate event not written");
    }

    [TestMethod]
    public void GetGeofence_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var geofenceType = GeofenceType.Filter;
      var geofenceUid = Guid.NewGuid();
      var createGeofenceEvent = new CreateGeofenceEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        GeofenceUID = geofenceUid,
        GeofenceName = "Boundary one",
        GeofenceType = geofenceType.ToString(),
        GeometryWKT = "POLYGON((80.257874 12.677856,79.856873 13.039345,80.375977 13.443052,80.257874 12.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      this.GeofenceRepo.StoreEvent(createGeofenceEvent).Wait();

      var g = this.GeofenceRepo.GetGeofence(geofenceUid.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve geofence from geofenceRepo");
      Assert.AreEqual(geofenceUid.ToString(), g.Result.GeofenceUID, "Wrong geofence retrieved");
    }

    [TestMethod]
    public void GetGeofences_HappyPath()
    {
      var custUid = Guid.NewGuid();
      var userId = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var geofenceType = GeofenceType.Filter.ToString();

      var createGeofenceEvent1 = new CreateGeofenceEvent
      {
        CustomerUID = custUid,
        UserUID = userId,
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Boundary one",
        GeofenceType = geofenceType,
        GeometryWKT = "POLYGON((80.257874 12.677856,79.856873 13.039345,80.375977 13.443052,80.257874 12.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var createGeofenceEvent2 = new CreateGeofenceEvent
      {
        CustomerUID = custUid,
        UserUID = userId,
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Boundary two",
        GeofenceType = geofenceType,
        GeometryWKT = "POLYGON((81.257874 13.677856,80.856873 14.039345,81.375977 14.443052,81.257874 13.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var createGeofenceEvent3 = new CreateGeofenceEvent
      {
        CustomerUID = custUid,
        UserUID = userId,
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Boundary three",
        GeofenceType = geofenceType,
        GeometryWKT = "POLYGON((82.257874 14.677856,81.856873 15.039345,82.375977 15.443052,82.257874 14.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var deleteGeofenceEvent = new DeleteGeofenceEvent
      {
        GeofenceUID = createGeofenceEvent1.GeofenceUID,
        UserUID = userId,
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      this.GeofenceRepo.StoreEvent(createGeofenceEvent1).Wait();
      this.GeofenceRepo.StoreEvent(createGeofenceEvent2).Wait();
      this.GeofenceRepo.StoreEvent(createGeofenceEvent3).Wait();
      this.GeofenceRepo.StoreEvent(deleteGeofenceEvent).Wait();

      var ids = new List<string>
      {
        createGeofenceEvent1.GeofenceUID.ToString(),
        createGeofenceEvent2.GeofenceUID.ToString(),
        createGeofenceEvent3.GeofenceUID.ToString()
      };
      var g = this.GeofenceRepo.GetGeofences(ids);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve geofences from geofenceRepo");
      Assert.AreEqual(2, g.Result.Count(), "Wrong number of geofences retrieved");
    }

    [TestMethod]
    public void CreateGeofence_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var geofenceType = GeofenceType.Filter;
      var createGeofenceEvent = new CreateGeofenceEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserUID = Guid.NewGuid(),
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Boundary one",
        GeofenceType = geofenceType.ToString(),
        GeometryWKT = "POLYGON((80.257874 12.677856,79.856873 13.039345,80.375977 13.443052,80.257874 12.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var boundary = new Geofence
      {
        CustomerUID = createGeofenceEvent.CustomerUID.ToString(),
        UserUID = createGeofenceEvent.UserUID.ToString(),
        GeofenceUID = createGeofenceEvent.GeofenceUID.ToString(),
        Name = createGeofenceEvent.GeofenceName,
        GeofenceType = geofenceType,
        GeometryWKT = createGeofenceEvent.GeometryWKT,
        LastActionedUTC = createGeofenceEvent.ActionUTC
      };

      WriteEventToDb(createGeofenceEvent, "Geofence event not written");

      var g = this.GeofenceRepo.GetGeofence(createGeofenceEvent.GeofenceUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve geofence from geofenceRepo");
      //Note: cannot compare objects as Geofence has nullables while CreateGeofenceEvent doesn't
      Assert.AreEqual(boundary.CustomerUID, g.Result.CustomerUID, "Wrong CustomerUID");
      Assert.AreEqual(boundary.UserUID, g.Result.UserUID, "Wrong UserUID");
      Assert.AreEqual(boundary.GeofenceUID, g.Result.GeofenceUID, "Wrong GeofenceUID");
      Assert.AreEqual(boundary.Name, g.Result.Name, "Wrong Name");
      Assert.AreEqual(boundary.GeofenceType, g.Result.GeofenceType, "Wrong GeofenceType");
      Assert.AreEqual(boundary.GeometryWKT, g.Result.GeometryWKT, "Wrong GeometryWKT");
    }

    [TestMethod]
    public void DeleteGeofence_HappyPath()
    {
      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var geofenceUid = Guid.NewGuid();
      var userUid = Guid.NewGuid();
      var geofenceType = GeofenceType.Filter;
      var createGeofenceEvent = new CreateGeofenceEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserUID = userUid,
        GeofenceUID = geofenceUid,
        GeofenceName = "Boundary one",
        GeofenceType = geofenceType.ToString(),
        GeometryWKT = "POLYGON((80.257874 12.677856,79.856873 13.039345,80.375977 13.443052,80.257874 12.677856))",
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var deleteGeofenceEvent = new DeleteGeofenceEvent
      {
        GeofenceUID = geofenceUid,
        UserUID = userUid,
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      this.GeofenceRepo.StoreEvent(createGeofenceEvent).Wait();

      WriteEventToDb(deleteGeofenceEvent, "Geofence event not deleted");

      var g = this.GeofenceRepo.GetGeofence(geofenceUid.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve geofence from geofenceRepo");
    }

    #endregion

    #region privates

    private void CheckSchema(string tableName, List<string> columnNames)
    {
      using (var connection = new MySqlConnection(this.ConfigStore.GetConnectionString("VSPDB")))
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
        what, this.ConfigStore.GetValueString("MYSQL_DATABASE_NAME"), tableName);
      return query;
    }

    #endregion privates
  }
}
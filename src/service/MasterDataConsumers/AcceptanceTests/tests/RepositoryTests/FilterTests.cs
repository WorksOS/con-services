using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests.Internal;
using System;
using System.Linq;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests
{
  [TestClass]
  public class FilterTests : TestControllerBase
  {
    FilterRepository filterRepo;

    [TestInitialize]
    public void Init()
    {
      SetupLogging();
      filterRepo = new FilterRepository(ServiceProvider.GetService<IConfigurationStore>(),
        ServiceProvider.GetService<ILoggerFactory>());
    }

    /// <summary>
    /// Get Happy path i.e. active, persistent only
    /// </summary>
    [TestMethod]
    public void GetFiltersForProject_PersistentOnly()
    {
      var custUid = Guid.NewGuid();
      var projUid = Guid.NewGuid();
      var userId = Guid.NewGuid();

      DateTime firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var createTransientFilterEvent1 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userId.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "",
        FilterJson = "blah1",
        FilterType = FilterType.Transient,
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };
      var createTransientFilterEvent2 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userId.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "Some name",
        FilterJson = "blah2",
        FilterType = FilterType.Report,
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createPersistentFilterEvent1 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userId.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "HasAName1",
        FilterJson = "blah1",
        FilterType = FilterType.Persistent,
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var createPersistentFilterEvent2 = new CreateFilterEvent()
      {
        CustomerUID = custUid,
        ProjectUID = projUid,
        UserID = userId.ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "HasAName2",
        FilterJson = "blah2",
        FilterType = FilterType.Persistent,
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      filterRepo.StoreEvent(createTransientFilterEvent1).Wait();
      filterRepo.StoreEvent(createTransientFilterEvent2).Wait();
      filterRepo.StoreEvent(createPersistentFilterEvent1).Wait();
      filterRepo.StoreEvent(createPersistentFilterEvent2).Wait();

      var g = filterRepo.GetFiltersForProject(projUid.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filters from filterRepo");
      Assert.AreEqual(2, g.Result.Count(), "retrieved filter count is incorrect");

      g = filterRepo.GetFiltersForProjectUser(custUid.ToString(), projUid.ToString(), userId.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filters from filterRepo");
      Assert.AreEqual(2, g.Result.Count(), "retrieved filter count is incorrect");

      var f = filterRepo.GetFilter(createTransientFilterEvent1.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createTransientFilterEvent1.FilterUID.ToString(), f.Result.FilterUid,
        "retrieved filter UId is incorrect");

      f = filterRepo.GetFilter(createTransientFilterEvent2.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createTransientFilterEvent2.FilterUID.ToString(), f.Result.FilterUid,
        "retrieved filter UId is incorrect");

      f = filterRepo.GetFilter(createPersistentFilterEvent1.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createPersistentFilterEvent1.FilterUID.ToString(), f.Result.FilterUid,
        "retrieved filter UId is incorrect");

      f = filterRepo.GetFilter(createPersistentFilterEvent2.FilterUID.ToString());
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(createPersistentFilterEvent2.FilterUID.ToString(), f.Result.FilterUid,
        "retrieved filter UId is incorrect");
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
        UserID = Guid.NewGuid().ToString(),
        ProjectUID = Guid.NewGuid(),
        FilterUID = Guid.NewGuid(),
        Name = "",
        FilterJson = "blah",
        FilterType = FilterType.Transient,
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
        FilterJson = createFilterEvent.FilterJson,
        FilterType = createFilterEvent.FilterType,
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
      Assert.AreEqual(0, f.Result.Count(), "retrieved filter count is incorrect");

      f = filterRepo.GetFiltersForProjectUser(createFilterEvent.CustomerUID.ToString(),
        createFilterEvent.ProjectUID.ToString(), createFilterEvent.UserID);
      f.Wait();
      Assert.IsNotNull(f.Result, "Unable to retrieve user filters from filterRepo");
      Assert.AreEqual(0, f.Result.Count(), "retrieved user filter count is incorrect");
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
        UserID = Guid.NewGuid().ToString(),
        FilterUID = Guid.NewGuid(),
        Name = "",
        FilterJson = "blah",
        FilterType = FilterType.Transient,
        ActionUTC = firstCreatedUtc,
        ReceivedUTC = firstCreatedUtc
      };

      var updateFilterEvent = new UpdateFilterEvent()
      {
        CustomerUID = createFilterEvent.CustomerUID,
        ProjectUID = createFilterEvent.ProjectUID,
        UserID = createFilterEvent.UserID,
        FilterUID = createFilterEvent.FilterUID,
        Name = "",
        FilterJson = "blahDeBlah",
        FilterType = FilterType.Transient,
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
        FilterJson = createFilterEvent.FilterJson,
        FilterType = createFilterEvent.FilterType,
        LastActionedUtc = createFilterEvent.ActionUTC
      };

      filterRepo.StoreEvent(createFilterEvent).Wait();

      var s = filterRepo.StoreEvent(updateFilterEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Filter event not updateable");

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve filter from filterRepo");
      Assert.AreEqual(filter, g.Result, "retrieved filter is incorrect");
    }
  }
}
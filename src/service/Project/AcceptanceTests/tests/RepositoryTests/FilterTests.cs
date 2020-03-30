using System;
using System.Linq;
using RepositoryTests.Internal;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Repository;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using Xunit;

namespace RepositoryTests
{
  public class FilterTests : TestControllerBase
  {
    FilterRepository filterRepo;

    public FilterTests()
    {
      SetupLogging();
      filterRepo = new FilterRepository(configStore, loggerFactory);
    }

    /// <summary>
    /// Get Happy path i.e. active, persistent only
    /// </summary>
    [Fact]
    public void GetFiltersForProject_PersistentOnly()
    {
      var custUid = Guid.NewGuid();
      var projUid = Guid.NewGuid();
      var userId = Guid.NewGuid();

      var firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
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
      Assert.NotNull(g.Result);
      Assert.Equal(2, g.Result.Count());

      g = filterRepo.GetFiltersForProjectUser(custUid.ToString(), projUid.ToString(), userId.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.Equal(2, g.Result.Count());

      var f = filterRepo.GetFilter(createTransientFilterEvent1.FilterUID.ToString());
      f.Wait();
      Assert.NotNull(f.Result);
      Assert.Equal(createTransientFilterEvent1.FilterUID.ToString(), f.Result.FilterUid);

      f = filterRepo.GetFilter(createTransientFilterEvent2.FilterUID.ToString());
      f.Wait();
      Assert.NotNull(f.Result);
      Assert.Equal(createTransientFilterEvent2.FilterUID.ToString(), f.Result.FilterUid);

      f = filterRepo.GetFilter(createPersistentFilterEvent1.FilterUID.ToString());
      f.Wait();
      Assert.NotNull(f.Result);
      Assert.Equal(createPersistentFilterEvent1.FilterUID.ToString(), f.Result.FilterUid);

      f = filterRepo.GetFilter(createPersistentFilterEvent2.FilterUID.ToString());
      f.Wait();
      Assert.NotNull(f.Result);
      Assert.Equal(createPersistentFilterEvent2.FilterUID.ToString(), f.Result.FilterUid);
    }

    /// <summary>
    /// Create Happy path i.e. filter doesn't exist
    /// </summary>
    [Fact]
    public void CreateTransientFilter_HappyPath()
    {
      var firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
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
      Assert.Equal(1, s.Result);

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.Equal(filter, g.Result);

      var f = filterRepo.GetFiltersForProject(createFilterEvent.ProjectUID.ToString());
      f.Wait();
      Assert.NotNull(f.Result);
      Assert.Empty(f.Result);

      f = filterRepo.GetFiltersForProjectUser(createFilterEvent.CustomerUID.ToString(),
        createFilterEvent.ProjectUID.ToString(), createFilterEvent.UserID);
      f.Wait();
      Assert.NotNull(f.Result);
      Assert.Empty(f.Result);
    }

    /// <summary>
    /// Update Happy path i.e. filter exists
    /// </summary>
    [Fact]
    public void UpdateTransientFilter_HappyPath()
    {
      var firstCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
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
      Assert.Equal(0, s.Result);

      var g = filterRepo.GetFilter(createFilterEvent.FilterUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.Equal(filter, g.Result);
    }
  }
}

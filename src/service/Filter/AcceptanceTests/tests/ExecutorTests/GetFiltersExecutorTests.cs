using ExecutorTests.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Newtonsoft.Json;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Linq;

namespace ExecutorTests
{
  [TestClass]
  public class GetFiltersExecutorTests : FilterRepositoryBase
  {
    [TestInitialize]
    public void ClassInit()
    {
      Setup();
    }

    [TestMethod]
    public async Task GetFiltersExecutor_NoExistingFilter()
    {
      var request = CreateAndValidateRequest();

      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(request) as FilterDescriptorListResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(0, result.FilterDescriptors.Count, "executor count is incorrect");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]

    public async Task GetFiltersExecutor_ExistingFilter(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "{\"dateRangeType\":0,\"elevationType\":null}";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterType = filterType,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(customerUid: custUid, userId: userId, projectUid: projectUid, filterJson: filterJson, filterType: filterType, name: name);

      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(request) as FilterDescriptorListResult;

      var filterToTest = new FilterDescriptorListResult
      {
        FilterDescriptors = ImmutableList<FilterDescriptor>.Empty.Add
          (new FilterDescriptor {FilterUid = filterUid, Name = name, FilterJson = filterJson, FilterType = filterType})
      };

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      var filters = result.FilterDescriptors.Where(f => f.FilterType == filterType).ToList();
      if (filterType == FilterType.Persistent)
      {
        Assert.AreEqual(1, filters.Count, "should be 1 filter");
        Assert.AreEqual(filterToTest.FilterDescriptors[0].FilterUid, filters[0].FilterUid,
          Responses.IncorrectFilterDescriptorFilterUid);
        Assert.AreEqual(filterToTest.FilterDescriptors[0].Name, filters[0].Name,
          Responses.IncorrectFilterDescriptorName);
        Assert.AreEqual("{\"dateRangeType\":0,\"dateRangeName\":\"Today\"}", filters[0].FilterJson,
          Responses.IncorrectFilterDescriptorFilterJson);
        Assert.AreEqual(filterToTest.FilterDescriptors[0].FilterType, filters[0].FilterType,
          Responses.IncorrectFilterDescriptorFilterType);
      }
      else
      {  
        Assert.AreEqual(0, filters.Count, "should be 0 filters");
      }
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public async Task GetFiltersExecutor_ExistingFilters2(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid1 = Guid.NewGuid().ToString();
      string filterUid2 = Guid.NewGuid().ToString();
      string name1 = "blah1";
      string name2 = "blah2";
      string filterJson1 = "{\"dateRangeType\":0,\"elevationType\":null}";
      string filterJson2 = "{\"dateRangeType\":1,\"elevationType\":null}";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid1),
        Name = name1,
        FilterType = filterType,
        FilterJson = filterJson1,
        ActionUTC = DateTime.UtcNow.AddMinutes(-5),
        ReceivedUTC = DateTime.UtcNow.AddMinutes(-5)
      });

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid2),
        Name = name2,
        FilterType = filterType,
        FilterJson = filterJson2,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(customerUid: custUid, userId: userId, projectUid: projectUid);

      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(request) as FilterDescriptorListResult;

      var filterToTest = new FilterDescriptorListResult
      {
        FilterDescriptors = ImmutableList<FilterDescriptor>.Empty
                                                           .Add(new FilterDescriptor { FilterUid = filterUid1, Name = name1, FilterType = filterType, FilterJson = filterJson1 })
                                                           .Add(new FilterDescriptor { FilterUid = filterUid2, Name = name2, FilterType = filterType, FilterJson = filterJson2 })
      };

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      if (filterType == FilterType.Persistent)
      {
        Assert.AreEqual(2, result.FilterDescriptors.Count, "should be 2 filter2");
        Assert.AreEqual(filterToTest.FilterDescriptors[0].FilterUid, result.FilterDescriptors[0].FilterUid,
          Responses.IncorrectFilterDescriptorFilterUid);
        Assert.AreEqual(filterToTest.FilterDescriptors[0].Name, result.FilterDescriptors[0].Name,
          Responses.IncorrectFilterDescriptorName);
        Assert.AreEqual("{\"dateRangeType\":0,\"dateRangeName\":\"Today\"}", result.FilterDescriptors[0].FilterJson,
          Responses.IncorrectFilterDescriptorFilterJson);
        Assert.AreEqual(filterToTest.FilterDescriptors[0].FilterType, result.FilterDescriptors[0].FilterType,
          Responses.IncorrectFilterDescriptorFilterType);
        Assert.AreEqual(filterToTest.FilterDescriptors[1].FilterUid, result.FilterDescriptors[1].FilterUid,
          Responses.IncorrectFilterDescriptorFilterUid);
        Assert.AreEqual(filterToTest.FilterDescriptors[1].Name, result.FilterDescriptors[1].Name,
          Responses.IncorrectFilterDescriptorName);
        Assert.AreEqual("{\"dateRangeType\":1,\"dateRangeName\":\"Yesterday\"}", result.FilterDescriptors[1].FilterJson,
          Responses.IncorrectFilterDescriptorFilterJson);
        Assert.AreEqual(filterToTest.FilterDescriptors[1].FilterType, result.FilterDescriptors[1].FilterType,
          Responses.IncorrectFilterDescriptorFilterType);
      }
      else
      {
        Assert.AreEqual(0, result.FilterDescriptors.Count, "should be 0 filter2");
      }
    }


    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public async Task GetFiltersExecutor_ExistingFilters2ApplicationContext(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid1 = Guid.NewGuid().ToString();
      string filterUid2 = Guid.NewGuid().ToString();
      string name1 = "blah1";
      string name2 = "blah2";
      string filterJson1 = "{\"dateRangeType\":0,\"elevationType\":null}";
      string filterJson2 = "{\"dateRangeType\":1,\"elevationType\":null}";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid1),
        Name = name1,
        FilterType = filterType,
        FilterJson = filterJson1,
        ActionUTC = DateTime.UtcNow.AddMinutes(-5),
        ReceivedUTC = DateTime.UtcNow.AddMinutes(-5)
      });

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid2),
        Name = name2,
        FilterType = filterType,
        FilterJson = filterJson2,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(customerUid: custUid, isApplicationContext: true, projectUid: projectUid);

      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(request) as FilterDescriptorListResult;

      var filterToTest = new FilterDescriptorListResult
      {
        FilterDescriptors = ImmutableList<FilterDescriptor>.Empty
                                                           .Add(new FilterDescriptor { FilterUid = filterUid1, Name = name1, FilterType = filterType, FilterJson = filterJson1 })
                                                           .Add(new FilterDescriptor { FilterUid = filterUid2, Name = name2, FilterType = filterType, FilterJson = filterJson2 })
      };

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      if (filterType == FilterType.Persistent)
      {
        Assert.AreEqual(2, result.FilterDescriptors.Count, "should be 2 filter2");
        Assert.AreEqual(filterToTest.FilterDescriptors[0].FilterUid, result.FilterDescriptors[0].FilterUid,
          Responses.IncorrectFilterDescriptorFilterUid);
        Assert.AreEqual(filterToTest.FilterDescriptors[0].Name, result.FilterDescriptors[0].Name,
          Responses.IncorrectFilterDescriptorName);
        Assert.AreEqual("{\"dateRangeType\":0,\"dateRangeName\":\"Today\"}", result.FilterDescriptors[0].FilterJson,
          Responses.IncorrectFilterDescriptorFilterJson);
        Assert.AreEqual(filterToTest.FilterDescriptors[0].FilterType, result.FilterDescriptors[0].FilterType,
          Responses.IncorrectFilterDescriptorFilterType);
        Assert.AreEqual(filterToTest.FilterDescriptors[1].FilterUid, result.FilterDescriptors[1].FilterUid,
          Responses.IncorrectFilterDescriptorFilterUid);
        Assert.AreEqual(filterToTest.FilterDescriptors[1].Name, result.FilterDescriptors[1].Name,
          Responses.IncorrectFilterDescriptorName);
        Assert.AreEqual("{\"dateRangeType\":1,\"dateRangeName\":\"Yesterday\"}", result.FilterDescriptors[1].FilterJson,
          Responses.IncorrectFilterDescriptorFilterJson);
        Assert.AreEqual(filterToTest.FilterDescriptors[1].FilterType, result.FilterDescriptors[1].FilterType,
          Responses.IncorrectFilterDescriptorFilterType);
      }
      else
      {
        Assert.AreEqual(0, result.FilterDescriptors.Count, "should be 0 filter2");
      }
    }

    [TestMethod]
    [DataRow(DateRangeType.ProjectExtents, true)]
    [DataRow(DateRangeType.ProjectExtents, false)]
    public void GetFiltersExecutor_Should_not_add_start_end_dates(int dateRangeType, bool asAtDate)
    {
      var filterCreateEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserID = TestUtility.UIDs.JWT_USER_ID,
        ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
        FilterUID = Guid.NewGuid(),
        Name = $"dateRangeType={dateRangeType},asAtDate={asAtDate}",
        FilterType = FilterType.Persistent,
        FilterJson = $"{{\"startUtc\": null,\"endUtc\": null,\"dateRangeType\": {dateRangeType}, \"asAtDate\":\"{asAtDate}\"}}",
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      WriteEventToDb(filterCreateEvent);

      var request = CreateAndValidateRequest(customerUid: filterCreateEvent.CustomerUID.ToString(), userId: filterCreateEvent.UserID, projectUid: filterCreateEvent.ProjectUID.ToString(), filterUid: filterCreateEvent.FilterUID.ToString());

      var projectData = new ProjectData { ProjectUid = filterCreateEvent.ProjectUID.ToString(), IanaTimeZone = "America/Los_Angeles" };

      var tcs = new TaskCompletionSource<List<ProjectData>>();
      tcs.SetResult(new List<ProjectData> { projectData });

      var projectListMock = new Mock<IProjectListProxy>();
      projectListMock.Setup(x => x.GetProjectsV4(filterCreateEvent.CustomerUID.ToString(), request.CustomHeaders)).Returns(() => tcs.Task);

      var projectProxy = new ProjectListProxy(this.ConfigStore, this.Logger, new MemoryCache(new MemoryCacheOptions()));
      var executor = RequestExecutorContainer.Build<GetFiltersExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null, projectProxy);
      var result = executor.ProcessAsync(request).Result as FilterDescriptorListResult;

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(filterCreateEvent.FilterUID, Guid.Parse(result.FilterDescriptors[0].FilterUid), Responses.IncorrectFilterDescriptorFilterUid);

      dynamic filterObj = JsonConvert.DeserializeObject(result.FilterDescriptors[0].FilterJson);
      Assert.IsNull(filterObj.startUtc);
      Assert.IsNull(filterObj.endUtc);
    }

    [TestMethod]
    //[DataRow(DateRangeType.Custom, true)]
    [DataRow(DateRangeType.Custom, false)]
    public async Task GetFiltersExecutor_Should_not_alter_existing_start_end_dates(int dateRangeType, bool asAtDate)
    {
      var customerUid = Guid.NewGuid();
      const string startDate = "2017-12-10T08:00:00Z";
      const string endDate = "2017-12-10T20:09:59.108671Z";

      var events = new List<CreateFilterEvent> {
        new CreateFilterEvent
        {
          CustomerUID = customerUid,
          UserID = TestUtility.UIDs.JWT_USER_ID,
          ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
          FilterUID = Guid.NewGuid(),
          Name = $"dateRangeType={dateRangeType},asAtDate={asAtDate}",
          FilterType = FilterType.Persistent,
          FilterJson = $"{{\"startUtc\": \"{startDate}\",\"endUtc\": \"{endDate}\",\"dateRangeType\": {dateRangeType}, \"asAtDate\":\"{asAtDate}\"}}",
          ActionUTC = DateTime.UtcNow,
          ReceivedUTC = DateTime.UtcNow
        },

        new CreateFilterEvent
        {
          CustomerUID = customerUid,
          UserID = TestUtility.UIDs.JWT_USER_ID,
          ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
          FilterUID = Guid.NewGuid(),
          Name = $"dateRangeType={dateRangeType}",
          FilterType = FilterType.Persistent,
          FilterJson = $"{{\"startUtc\": \"{startDate}\",\"endUtc\": \"{endDate}\",\"dateRangeType\": {dateRangeType}}}",
          ActionUTC = DateTime.UtcNow,
          ReceivedUTC = DateTime.UtcNow
        }};

      WriteEventToDb(events[0]);
      WriteEventToDb(events[1]);



      var request = CreateAndValidateRequest(customerUid: events[0].CustomerUID.ToString(), userId: events[0].UserID, projectUid: events[0].ProjectUID.ToString(), filterUid: events[0].FilterUID.ToString());

      var projectData = new ProjectData { ProjectUid = events[0].ProjectUID.ToString(), IanaTimeZone = "America/Los_Angeles" };

      var tcs = new TaskCompletionSource<List<ProjectData>>();
      tcs.SetResult(new List<ProjectData> { projectData });

      var projectListMock = new Mock<IProjectListProxy>();
      projectListMock.Setup(x => x.GetProjectsV4(events[0].CustomerUID.ToString(), request.CustomHeaders)).Returns(() => tcs.Task);

      var projectProxy = new ProjectListProxy(this.ConfigStore, this.Logger, new MemoryCache(new MemoryCacheOptions()));
      var executor = RequestExecutorContainer.Build<GetFiltersExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null, projectProxy);
      var result = await executor.ProcessAsync(request) as FilterDescriptorListResult;

      Assert.IsNotNull(result);

      for (var i = 0; i < events.Count; i++)
      {
        Assert.IsNotNull(result, Responses.ShouldReturnResult);
        Assert.AreEqual(events[i].FilterUID, Guid.Parse(result.FilterDescriptors[i].FilterUid), Responses.IncorrectFilterDescriptorFilterUid);

        dynamic filterObj = JsonConvert.DeserializeObject(result.FilterDescriptors[i].FilterJson);
        if (asAtDate)
          Assert.IsNull(filterObj.startUtc);
        else
          Assert.AreEqual(DateTime.Parse(startDate).ToUniversalTime(), DateTime.Parse(filterObj.startUtc.ToString()));

        Assert.AreEqual(DateTime.Parse(endDate).ToUniversalTime().ToString(), filterObj.endUtc.ToString());
      }
    }

    [TestMethod]
    [DataRow(DateRangeType.Today, true)]
    [DataRow(DateRangeType.Yesterday, true)]
    [DataRow(DateRangeType.CurrentWeek, true)]
    [DataRow(DateRangeType.CurrentMonth, true)]
    [DataRow(DateRangeType.PreviousWeek, true)]
    [DataRow(DateRangeType.PreviousMonth, true)]
    [DataRow(DateRangeType.PriorToYesterday, true)]
    [DataRow(DateRangeType.PriorToPreviousWeek, true)]
    [DataRow(DateRangeType.PriorToPreviousMonth, true)]
    [DataRow(DateRangeType.Today, false)]
    [DataRow(DateRangeType.Yesterday, false)]
    [DataRow(DateRangeType.CurrentWeek, false)]
    [DataRow(DateRangeType.CurrentMonth, false)]
    [DataRow(DateRangeType.PreviousWeek, false)]
    [DataRow(DateRangeType.PreviousMonth, false)]
    [DataRow(DateRangeType.PriorToYesterday, false)]
    [DataRow(DateRangeType.PriorToPreviousWeek, false)]
    [DataRow(DateRangeType.PriorToPreviousMonth, false)]
    public void GetFiltersExecutor_Should_add_start_end_dates(int dateRangeType, bool asAtDate)
    {
      var filterCreateEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserID = TestUtility.UIDs.JWT_USER_ID,
        ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
        FilterUID = Guid.NewGuid(),
        Name = $"dateRangeType={dateRangeType},asAtDate={asAtDate}",
        FilterType = FilterType.Persistent,
        FilterJson = $"{{\"startUtc\": null,\"endUtc\": null,\"dateRangeType\": {dateRangeType}, \"asAtDate\":\"{asAtDate}\"}}",
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      WriteEventToDb(filterCreateEvent);

      var projectData = new ProjectData { ProjectUid = filterCreateEvent.ProjectUID.ToString(), IanaTimeZone = "America/Los_Angeles" };

      var request = CreateAndValidateRequest(customerUid: filterCreateEvent.CustomerUID.ToString(), userId: filterCreateEvent.UserID, projectData: projectData, filterUid: filterCreateEvent.FilterUID.ToString());
    
      var tcs = new TaskCompletionSource<List<ProjectData>>();
      tcs.SetResult(new List<ProjectData> {projectData});

      var projectListMock = new Mock<IProjectListProxy>();
      projectListMock.Setup(x => x.GetProjectsV4(filterCreateEvent.CustomerUID.ToString(), request.CustomHeaders)).Returns(() => tcs.Task);

      var projectProxy = new ProjectListProxy(this.ConfigStore, this.Logger, new MemoryCache(new MemoryCacheOptions()));
      var executor = RequestExecutorContainer.Build<GetFiltersExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null, projectProxy);
      var result = executor.ProcessAsync(request).Result as FilterDescriptorListResult;

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(filterCreateEvent.FilterUID, Guid.Parse(result.FilterDescriptors[0].FilterUid), Responses.IncorrectFilterDescriptorFilterUid);

      dynamic filterObj = JsonConvert.DeserializeObject(result.FilterDescriptors[0].FilterJson);
      if (asAtDate)
        Assert.IsNull(filterObj.StartUtc);
      else
        Assert.IsTrue(DateTime.TryParse(filterObj.startUtc.ToString(), out DateTime _));
      Assert.IsTrue(DateTime.TryParse(filterObj.endUtc.ToString(), out DateTime _));
    }

 
  }
}
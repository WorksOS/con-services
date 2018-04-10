using ExecutorTests.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class GetFilterExecutorTests : FilterRepositoryBase
  {
    [TestInitialize]
    public void ClassInit()
    {
      Setup();
    }

    [TestMethod]
    [DataRow(FilterType.Persistent, "some filter")]
    [DataRow(FilterType.Transient, "")]
    [DataRow(FilterType.Report, "another filter")]
    public async Task GetFilterExecutor_NoExistingFilter(FilterType filterType, string name)
    {
      var request = CreateAndValidateRequest(filterType: filterType, name: name);

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2036", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("GetFilter By filterUid. The requested filter does not exist, or does not belong to the requesting customer; project or user.", StringComparison.Ordinal), "executor threw exception but incorrect message");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public async Task GetFilterExecutor_ExistingFilter(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "{\"dateRangeType\":1,\"elevationType\":null}";

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

      var request = FilterRequestFull.Create(null, custUid, false, userId,  new ProjectData() { ProjectUid = projectUid }, new FilterRequest {FilterUid = filterUid, FilterType = filterType});

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(request).ConfigureAwait(false) as FilterDescriptorSingleResult;

      var filterToTest = new FilterDescriptorSingleResult(
        new FilterDescriptor
        {
          FilterUid = filterUid,
          Name = name,
          FilterType = filterType,
          FilterJson = filterJson
        }
      );

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterUid, result.FilterDescriptor.FilterUid,
        Responses.IncorrectFilterDescriptorFilterUid);
      Assert.AreEqual(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name,
        Responses.IncorrectFilterDescriptorName);
      Assert.AreEqual("{\"dateRangeType\":1,\"dateRangeName\":\"Yesterday\"}", result.FilterDescriptor.FilterJson,
        Responses.IncorrectFilterDescriptorFilterJson);
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType,
        Responses.IncorrectFilterDescriptorFilterType);

    }


    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public async Task GetFilterExecutor_ExistingFilterApplicationContext(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "{\"dateRangeType\":1,\"elevationType\":null}";

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

      var request = FilterRequestFull.Create(null, custUid, true, "0", new ProjectData() { ProjectUid = projectUid }, new FilterRequest { FilterUid = filterUid, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(request).ConfigureAwait(false) as FilterDescriptorSingleResult;

      var filterToTest = new FilterDescriptorSingleResult(
        new FilterDescriptor
        {
          FilterUid = filterUid,
          Name = name,
          FilterType = filterType,
          FilterJson = filterJson
        }
      );

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterUid, result.FilterDescriptor.FilterUid,
        Responses.IncorrectFilterDescriptorFilterUid);
      Assert.AreEqual(filterToTest.FilterDescriptor.Name, result.FilterDescriptor.Name,
        Responses.IncorrectFilterDescriptorName);
      Assert.AreEqual("{\"dateRangeType\":1,\"dateRangeName\":\"Yesterday\"}", result.FilterDescriptor.FilterJson,
        Responses.IncorrectFilterDescriptorFilterJson);
      Assert.AreEqual(filterToTest.FilterDescriptor.FilterType, result.FilterDescriptor.FilterType,
        Responses.IncorrectFilterDescriptorFilterType);
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public async Task GetFilterExecutor_ExistingFilter_CaseInsensitive(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString().ToLower(); // actually Guid.Parse converts to lower anyway
      string userId = Guid.NewGuid().ToString().ToLower();
      string projectUid = Guid.NewGuid().ToString().ToLower();
      string filterUid = Guid.NewGuid().ToString().ToLower();
      string name = "blah";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterType = filterType,
        FilterJson = "{\"dateRangeType\":0,\"elevationType\":null}",
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(customerUid: custUid.ToUpper(), userId: userId.ToUpper(), projectUid: projectUid.ToUpper(), filterUid: filterUid.ToUpper(), filterType: filterType, name: name);

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(filterUid, result.FilterDescriptor.FilterUid, Responses.IncorrectFilterDescriptorFilterUid);
      Assert.AreEqual(name, result.FilterDescriptor.Name, Responses.IncorrectFilterDescriptorName);
      Assert.AreEqual("{\"dateRangeType\":0,\"dateRangeName\":\"Today\"}", result.FilterDescriptor.FilterJson, Responses.IncorrectFilterDescriptorFilterJson);
      Assert.AreEqual(filterType, result.FilterDescriptor.FilterType, Responses.IncorrectFilterDescriptorFilterType);
    }

    [TestMethod]
    [DataRow(DateRangeType.ProjectExtents)]
    [DataRow(DateRangeType.Custom)]
    public void GetFilterExecutor_Should_not_add_start_end_dates(int dateRangeType)
    {
      var filterType = FilterType.Transient;
      var name = $"dateRangeType={dateRangeType}";

      var filterCreateEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserID = TestUtility.UIDs.JWT_USER_ID,
        ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
        FilterUID = Guid.NewGuid(),
        Name = name,
        FilterType = filterType,
        FilterJson = $"{{\"startUtc\": null,\"endUtc\": null,\"dateRangeType\": {dateRangeType}}}",
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      WriteEventToDb(filterCreateEvent);

      var request = CreateAndValidateRequest(customerUid: filterCreateEvent.CustomerUID.ToString(), userId: filterCreateEvent.UserID, projectUid: filterCreateEvent.ProjectUID.ToString(), filterUid: filterCreateEvent.FilterUID.ToString(), filterType: filterType, name: name);

      var projectData = new ProjectData { ProjectUid = filterCreateEvent.ProjectUID.ToString(), IanaTimeZone = "America/Los_Angeles" };

      var tcs = new TaskCompletionSource<List<ProjectData>>();
      tcs.SetResult(new List<ProjectData> { projectData });

      var projectListMock = new Mock<IProjectListProxy>();
      projectListMock.Setup(x => x.GetProjectsV4(filterCreateEvent.CustomerUID.ToString(), request.CustomHeaders)).Returns(() => tcs.Task);

      var projectProxy = new ProjectListProxy(this.ConfigStore, this.Logger, new MemoryCache(new MemoryCacheOptions()));
      var executor = RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null, projectProxy);
      var result = executor.ProcessAsync(request).Result as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(filterCreateEvent.FilterUID, Guid.Parse(result.FilterDescriptor.FilterUid), Responses.IncorrectFilterDescriptorFilterUid);

      dynamic filterObj = JsonConvert.DeserializeObject(result.FilterDescriptor.FilterJson);
      Assert.IsNull(filterObj.startUtc);
      Assert.IsNull(filterObj.endUtc);
    }

    [TestMethod]
    [DataRow(DateRangeType.Today)]
    [DataRow(DateRangeType.Yesterday)]
    [DataRow(DateRangeType.CurrentWeek)]
    [DataRow(DateRangeType.CurrentMonth)]
    [DataRow(DateRangeType.PreviousWeek)]
    [DataRow(DateRangeType.PreviousMonth)]
    public void GetFilterExecutor_Should_add_start_end_dates(int dateRangeType)
    {
      var filterType = FilterType.Transient;
      var name = $"dateRangeType={dateRangeType}";

      var filterCreateEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserID = TestUtility.UIDs.JWT_USER_ID,
        ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
        FilterUID = Guid.NewGuid(),
        Name = name,
        FilterType = filterType,
        FilterJson = $"{{\"startUtc\": null,\"endUtc\": null,\"dateRangeType\": {dateRangeType}}}",
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      WriteEventToDb(filterCreateEvent);

      var projectData = new ProjectData { ProjectUid = filterCreateEvent.ProjectUID.ToString(), IanaTimeZone = "America/Los_Angeles" };

      var request = CreateAndValidateRequest(customerUid: filterCreateEvent.CustomerUID.ToString(), userId: filterCreateEvent.UserID, projectData: projectData, filterUid: filterCreateEvent.FilterUID.ToString(), filterType: filterType, name: name);

      var tcs = new TaskCompletionSource<List<ProjectData>>();
      tcs.SetResult(new List<ProjectData> {projectData});

      var projectListMock = new Mock<IProjectListProxy>();
      projectListMock.Setup(x => x.GetProjectsV4(filterCreateEvent.CustomerUID.ToString(), request.CustomHeaders)).Returns(() => tcs.Task);

      var projectProxy = new ProjectListProxy(this.ConfigStore, this.Logger, new MemoryCache(new MemoryCacheOptions()));
      var executor = RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null, projectProxy);
      var result = executor.ProcessAsync(request).Result as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(filterCreateEvent.FilterUID, Guid.Parse(result.FilterDescriptor.FilterUid), Responses.IncorrectFilterDescriptorFilterUid);

      Filter filterObj = JsonConvert.DeserializeObject<Filter>(result.FilterDescriptor.FilterJson);
      Assert.IsTrue(DateTime.TryParse(filterObj.StartUtc.ToString(), out DateTime _));
      Assert.IsTrue(DateTime.TryParse(filterObj.EndUtc.ToString(), out DateTime _));
    }

    [TestMethod]
    [DataRow(DateRangeType.ProjectExtents)]
    [DataRow(DateRangeType.Custom)]
    public async Task GetFilterExecutor_Should_not_alter_existing_start_end_dates(int dateRangeType)
    {
      const string startDate = "2017-12-10T08:00:00Z";
      const string endDate = "2017-12-10T20:09:59.108671Z";

      var filterType = FilterType.Transient;
      var name = $"dateRangeType={dateRangeType}";

      var filterCreateEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.NewGuid(),
        UserID = TestUtility.UIDs.JWT_USER_ID,
        ProjectUID = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID,
        FilterUID = Guid.NewGuid(),
        Name = name,
        FilterType = filterType,
        FilterJson = $"{{\"startUtc\": \"{startDate}\",\"endUtc\": \"{endDate}\",\"dateRangeType\": {dateRangeType}}}",
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      WriteEventToDb(filterCreateEvent);

      var request = CreateAndValidateRequest(customerUid: filterCreateEvent.CustomerUID.ToString(), userId: filterCreateEvent.UserID, projectUid: filterCreateEvent.ProjectUID.ToString(), filterUid: filterCreateEvent.FilterUID.ToString(), filterType: filterType, name: name);

      var projectData = new ProjectData { ProjectUid = filterCreateEvent.ProjectUID.ToString(), IanaTimeZone = "America/Los_Angeles" };

      var tcs = new TaskCompletionSource<List<ProjectData>>();
      tcs.SetResult(new List<ProjectData> { projectData });

      var projectListMock = new Mock<IProjectListProxy>();
      projectListMock.Setup(x => x.GetProjectsV4(filterCreateEvent.CustomerUID.ToString(), request.CustomHeaders)).Returns(() => tcs.Task);

      var projectProxy = new ProjectListProxy(this.ConfigStore, this.Logger, new MemoryCache(new MemoryCacheOptions()));
      var executor = RequestExecutorContainer.Build<GetFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null, projectProxy);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(filterCreateEvent.FilterUID, Guid.Parse(result.FilterDescriptor.FilterUid), Responses.IncorrectFilterDescriptorFilterUid);

      dynamic filterObj = JsonConvert.DeserializeObject(result.FilterDescriptor.FilterJson);
      Assert.AreEqual(DateTime.Parse(startDate).ToUniversalTime(), DateTime.Parse(filterObj.startUtc.ToString()));
    }
  }
}
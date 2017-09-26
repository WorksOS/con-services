using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.MasterData.Models.Models;

namespace ExecutorTests
{
  [TestClass]
  public class GetFiltersExecutorTests : TestControllerBase
  {

    [TestInitialize]
    public void Init()
    {
      SetupDI();
    }

    [TestMethod]
    public async System.Threading.Tasks.Task GetFiltersExecutor_NoExistingFilter()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();

      var request = FilterRequestFull.Create(custUid, false, userUid, projectUid, filterUid);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(configStore, logger, serviceExceptionHandler, filterRepo);
      var result = await executor.ProcessAsync(request).ConfigureAwait(false) as FilterDescriptorListResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(0, result.filterDescriptors.Count, "executor count is incorrect");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task GetFiltersExecutor_ExistingFilter()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      var s = filterRepo.StoreEvent(createFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event not written");
      

      var request = FilterRequestFull.Create(custUid, false, userId, projectUid);
      request.Validate(serviceExceptionHandler);
      
      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(configStore, logger, serviceExceptionHandler, filterRepo);
      var result = await executor.ProcessAsync(request).ConfigureAwait(false) as FilterDescriptorListResult;

      var filterToTest = new FilterDescriptorListResult
      {
        filterDescriptors = ImmutableList<FilterDescriptor>.Empty.Add
        ( new FilterDescriptor() { FilterUid = filterUid, Name = name, FilterJson = filterJson })
      };


      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(1, result.filterDescriptors.Count, "should be 1 filter");
      Assert.AreEqual(filterToTest.filterDescriptors[0].FilterUid, result.filterDescriptors[0].FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.filterDescriptors[0].Name, result.filterDescriptors[0].Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterToTest.filterDescriptors[0].FilterJson, result.filterDescriptors[0].FilterJson,
        "executor returned incorrect filterDescriptor FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task GetFiltersExecutor_ExistingFilters2()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid1 = Guid.NewGuid().ToString();
      string filterUid2 = Guid.NewGuid().ToString();
      string name1 = "blah1";
      string name2 = "blah2";
      string filterJson1 = "theJsonString1";
      string filterJson2 = "theJsonString2";

      var createFilterEvent1 = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid1),
        Name = name1,
        FilterJson = filterJson1,
        ActionUTC = DateTime.UtcNow.AddMinutes(-5),
        ReceivedUTC = DateTime.UtcNow.AddMinutes(-5),
      };
      var createFilterEvent2 = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid2),
        Name = name2,
        FilterJson = filterJson2,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      filterRepo.StoreEvent(createFilterEvent1).Wait();
      filterRepo.StoreEvent(createFilterEvent2).Wait();


      var request = FilterRequestFull.Create(custUid, false, userId, projectUid);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(configStore, logger, serviceExceptionHandler, filterRepo);
      var result = await executor.ProcessAsync(request).ConfigureAwait(false) as FilterDescriptorListResult;

      var filterToTest = new FilterDescriptorListResult
      {
        filterDescriptors = ImmutableList<FilterDescriptor>.Empty
          .Add(new FilterDescriptor() { FilterUid = filterUid1, Name = name1, FilterJson = filterJson1 })
          .Add(new FilterDescriptor() { FilterUid = filterUid2, Name = name2, FilterJson = filterJson2 })
      };
      
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(2, result.filterDescriptors.Count, "should be 2 filter2");
      Assert.AreEqual(filterToTest.filterDescriptors[0].FilterUid, result.filterDescriptors[0].FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.filterDescriptors[0].Name, result.filterDescriptors[0].Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterToTest.filterDescriptors[0].FilterJson, result.filterDescriptors[0].FilterJson,
        "executor returned incorrect filterDescriptor FilterJson");
      Assert.AreEqual(filterToTest.filterDescriptors[1].FilterUid, result.filterDescriptors[1].FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.filterDescriptors[1].Name, result.filterDescriptors[1].Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterToTest.filterDescriptors[1].FilterJson, result.filterDescriptors[1].FilterJson,
        "executor returned incorrect filterDescriptor FilterJson");
    }
  }
}

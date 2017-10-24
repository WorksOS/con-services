using ExecutorTests.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
    public async Task GetFiltersExecutor_ExistingFilter()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(customerUid: custUid, userId: userId, projectUid: projectUid);

      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null);
      var result = await executor.ProcessAsync(request) as FilterDescriptorListResult;

      var filterToTest = new FilterDescriptorListResult
      {
        FilterDescriptors = ImmutableList<FilterDescriptor>.Empty.Add
        (new FilterDescriptor { FilterUid = filterUid, Name = name, FilterJson = filterJson })
      };

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(1, result.FilterDescriptors.Count, "should be 1 filter");
      Assert.AreEqual(filterToTest.FilterDescriptors[0].FilterUid, result.FilterDescriptors[0].FilterUid, Responses.IncorrectFilterDescriptorFilterUid);
      Assert.AreEqual(filterToTest.FilterDescriptors[0].Name, result.FilterDescriptors[0].Name, Responses.IncorrectFilterDescriptorName);
      Assert.AreEqual(filterToTest.FilterDescriptors[0].FilterJson, result.FilterDescriptors[0].FilterJson, Responses.IncorrectFilterDescriptorFilterJson);
    }

    [TestMethod]
    public async Task GetFiltersExecutor_ExistingFilters2()
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

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid1),
        Name = name1,
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
          .Add(new FilterDescriptor { FilterUid = filterUid1, Name = name1, FilterJson = filterJson1 })
          .Add(new FilterDescriptor { FilterUid = filterUid2, Name = name2, FilterJson = filterJson2 })
      };

      Assert.IsNotNull(result, Responses.ShouldReturnResult);
      Assert.AreEqual(2, result.FilterDescriptors.Count, "should be 2 filter2");
      Assert.AreEqual(filterToTest.FilterDescriptors[0].FilterUid, result.FilterDescriptors[0].FilterUid,
        Responses.IncorrectFilterDescriptorFilterUid);
      Assert.AreEqual(filterToTest.FilterDescriptors[0].Name, result.FilterDescriptors[0].Name,
        Responses.IncorrectFilterDescriptorName);
      Assert.AreEqual(filterToTest.FilterDescriptors[0].FilterJson, result.FilterDescriptors[0].FilterJson,
        Responses.IncorrectFilterDescriptorFilterJson);
      Assert.AreEqual(filterToTest.FilterDescriptors[1].FilterUid, result.FilterDescriptors[1].FilterUid,
        Responses.IncorrectFilterDescriptorFilterUid);
      Assert.AreEqual(filterToTest.FilterDescriptors[1].Name, result.FilterDescriptors[1].Name,
        Responses.IncorrectFilterDescriptorName);
      Assert.AreEqual(filterToTest.FilterDescriptors[1].FilterJson, result.FilterDescriptors[1].FilterJson,
        Responses.IncorrectFilterDescriptorFilterJson);
    }
  }
}
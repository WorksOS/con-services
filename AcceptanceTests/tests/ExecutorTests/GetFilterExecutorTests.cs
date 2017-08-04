using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class GetFilterExecutorTests : TestControllerBase
  {

    [TestInitialize]
    public void Init()
    {
      SetupDI();
    }

    [TestMethod]
    public async System.Threading.Tasks.Task GetFilterExecutor_NoExistingFilter()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;
      
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNull(result.filterDescriptor.FilterUid, "executor returned incorrect filterDescriptor FilterUid");
      Assert.IsNull(result.filterDescriptor.Name, "executor returned incorrect filterDescriptor Name");
      Assert.IsNull(result.filterDescriptor.FilterJson, "executor returned incorrect filterDescriptor FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task GetFilterExecutor_ExistingFilter()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserUID = Guid.Parse(userUid),
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
      

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid);
      request.Validate(serviceExceptionHandler);
      
      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      var filterToTest = new FilterDescriptorSingleResult(
        new FilterDescriptor()
        {
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson
        }
      );

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterUid, result.filterDescriptor.FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.filterDescriptor.Name, result.filterDescriptor.Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterJson, result.filterDescriptor.FilterJson,
        "executor returned incorrect filterDescriptor FilterJson");
    }
  }
}

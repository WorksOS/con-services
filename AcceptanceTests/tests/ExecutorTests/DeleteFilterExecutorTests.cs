using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class DeleteFilterExecutorTests : TestControllerBase
  {

    [TestInitialize]
    public void Init()
    {
      SetupDI();
    }

    [TestMethod]
    public async System.Threading.Tasks.Task DeleteFilterExecutor_NoExistingFilter()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<DeleteFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () =>  await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("3011", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("DeleteFilter failed. Unable to find filterUid.", StringComparison.Ordinal), "executor threw exception but incorrect message");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task DeleteFilterExecutor_ExistingFilter()
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
        RequestExecutorContainer.Build<DeleteFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as ContractExecutionResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(0, result.Code, "executor returned incorrect code");
      Assert.AreEqual("success", result.Message, "executor returned incorrect message");
    }
  }
}

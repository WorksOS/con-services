using ExecutorTests.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class DeleteFilterExecutorTests : FilterRepositoryBase
  {
    [TestInitialize]
    public void ClassInit()
    {
      Setup();
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public async Task DeleteFilterExecutor_NoExistingFilter(FilterType filterType)
    {
      var request = CreateAndValidateRequest(name:"delete " + filterType, filterType: filterType, onlyFilterUid: true);

      var executor =
        RequestExecutorContainer.Build<DeleteFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2011", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("DeleteFilter failed. The requested filter does not exist, or does not belong to the requesting customer; project or user.", StringComparison.Ordinal), "executor threw exception but incorrect message");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Transient)]
    [DataRow(FilterType.Report)]
    public async Task DeleteFilterExecutor_ExistingFilter(FilterType filterType)
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
        FilterType = filterType,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = CreateAndValidateRequest(name: name, customerUid: custUid, userId: userId, projectUid: projectUid, filterUid: filterUid, filterType: filterType, onlyFilterUid: true);

      var executor =
        RequestExecutorContainer.Build<DeleteFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, null, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(request);

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(0, result.Code, "executor returned incorrect code");
      Assert.AreEqual("success", result.Message, "executor returned incorrect message");
    }
  }
}
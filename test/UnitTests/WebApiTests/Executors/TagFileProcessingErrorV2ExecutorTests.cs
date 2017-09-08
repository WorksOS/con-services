using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using Moq;

namespace WebApiTests.Executors
{
  [TestClass]
  public class TagFileProcessingErrorV2ExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public async Task CanCallTagFileProcessingErrorV2Executor_tccOrgIdNotFound()
    {
      string tccOrgId = Guid.NewGuid().ToString();
      long? assetId = null;
      long? projectId = null;
      int error = 1;
      string tagFileName = "Machine Name--whatever --161230235959";
      string deviceSerialNumber = null;

      string customerUid = Guid.NewGuid().ToString();
      string assetUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();

      var tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, assetId, projectId,
        error, tagFileName,
        deviceSerialNumber);
      tagFileProcessingErrorRequest.Validate();
      var customerRepo = new Mock<ICustomerRepository>();
      var customerTccOrg = new CustomerTccOrg() { Name = "theName", CustomerType = VSS.VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer, CustomerUID = customerUid, TCCOrgID = tccOrgId};
      customerRepo.Setup(c => c.GetCustomerWithTccOrg(It.IsAny<string>())).ReturnsAsync(customerTccOrg);

      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor =
        RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(
          loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>(), assetRepository, deviceRepository,
          customerRepo.Object, projectRepository, subscriptionsRepository);
      var result = await executor.ProcessAsync(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsFalse(result.Result, "executor processed TagFileProcessingError");
    }

    //[TestMethod]
    //public async Task CanCallTagFileProcessingErrorExecutorValidInputWithError()
    //{
    //  TagFileProcessingErrorV2Request tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest(345345, "Machine Name--whatever --161230235959", -2);
    //  var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
    //  ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

    //  var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>());
    //  var result = await executor.ProcessAsync(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

    //  Assert.IsNotNull(result, "executor returned nothing");
    //  Assert.IsTrue(result.Result, "executor process TagFileProcessingError without error");
    //}

    //[TestMethod]
    //public async Task CanCallTagFileProcessingErrorExecutorValidInputWithoutError()
    //{
    //  TagFileProcessingErrorV2Request tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest(123, "Machine Name--whatever --161230235959", 3);
    //  var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
    //  ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

    //  var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>());
    //  var result = await executor.ProcessAsync(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

    //  Assert.IsNotNull(result, "executor returned nothing");
    //  Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError with error");
    //}

    //[TestMethod]
    //public void CanCallTagFileProcessingErrorExecutorValidInput2WithoutError()
    //{
    //  TagFileProcessingErrorV2Request tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest(123, "Machine Name--whatever --161230235959", 2);
    //  var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
    //  ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

    //  var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>());
    //  var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

    //  Assert.IsNotNull(result, "executor returned nothing");
    //  Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError with error");
    //}

    //[TestMethod]
    //public void CanCallGetTagFileProcessingErrorExecutorWithLegacyAssetId()
    //{
    //  long legacyAssetID = 46534636436;
    //  string tagFileName = "Machine Name--whatever --161230235959";
    //  TagFileErrorsEnum error = TagFileErrorsEnum.CoordConversion_Failure;
    //  var eventkeyDate = DateTime.UtcNow;
    //  TagFileProcessingErrorV2Request tagFileProcessingErrorRequest = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest(legacyAssetID, tagFileName, (int)error);

    //  var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
    //  ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

    //  var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorV2ExecutorTests>());
    //  var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

    //  Assert.IsNotNull(result, "executor returned nothing");
    //  Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError");
    //}
  }
}

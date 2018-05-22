using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests.Executors
{
  [TestClass]
  public class TagFileProcessingErrorV1ExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallTagFileProcessingErrorV1Executor_NoValidInput()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(-1, "Machine Name--whatever --161230235959", 0);
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsFalse(result.Result, "executor processed TagFileProcessingError");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorV1Executor_ValidInputWithError()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(345345, "Machine Name--whatever --161230235959", -2);
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor process TagFileProcessingError without error");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorV1Executor_ValidInputWithoutError()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(123, "Machine Name--whatever --161230235959", 3);
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError with error");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorV1Executor_ValidInput2WithoutError()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(123, "Machine Name--whatever --161230235959", 2);
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError with error");
    }

    [TestMethod]
    public void CanCallGetTagFileProcessingErrorV1Executor_WithLegacyAssetId()
    {
      long legacyAssetID = 46534636436;
      string tagFileName = "Machine Name--whatever --161230235959";
      TagFileErrorsEnum error = TagFileErrorsEnum.CoordConversion_Failure;
      var eventkeyDate = DateTime.UtcNow;
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(legacyAssetID, tagFileName, (int)error);

      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError");
    }
  }
}

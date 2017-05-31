using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;
using Repositories;
using WebApiModels.Models;
using WebApiModels.ResultHandling;
using WebApiModels.Executors;
using WebApiModels.Enums;

namespace WebApiTests.Executors
{
  [TestClass]
  public class TagFileProcessingErrorExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorNoValidInput()
    {
      TagFileProcessingErrorRequest tagFileProcessingErrorRequest = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(-1, "", 0);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var tfaResult = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(tfaResult, "executor returned nothing");
      Assert.IsFalse(tfaResult.Result, "executor processed TagFileProcessingError");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorValidInputWithError()
    {
      TagFileProcessingErrorRequest tagFileProcessingErrorRequest = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(345345, "theFileName", -2);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var tfaResult = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(tfaResult, "executor returned nothing");
      Assert.IsTrue(tfaResult.Result, "executor process TagFileProcessingError without error");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorValidInputWithoutError()
    {
      TagFileProcessingErrorRequest tagFileProcessingErrorRequest = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(123, "Who Cares.tag", 3);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var tfaResult = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(tfaResult, "executor returned nothing");
      Assert.IsTrue(tfaResult.Result, "executor didn't process TagFileProcessingError with error");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorValidInput2WithoutError()
    {
      TagFileProcessingErrorRequest tagFileProcessingErrorRequest = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(123, "Who Cares.tag", 2);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var tfaResult = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(tfaResult, "executor returned nothing");
      Assert.IsTrue(tfaResult.Result, "executor didn't process TagFileProcessingError with error");
    }

    [TestMethod]
    public void CanCallGetTagFileProcessingErrorExecutorWithLegacyAssetId()
    {
      long legacyAssetID = 46534636436;
      string tagFileName = "Whatever";
      TagFileErrorsEnum error = TagFileErrorsEnum.CoordConversion_Failure;
      var eventkeyDate = DateTime.UtcNow;
      TagFileProcessingErrorRequest tagFileProcessingErrorRequest = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(legacyAssetID, tagFileName, (int)error);

      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var tfaResult = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(tfaResult, "executor returned nothing");
      Assert.IsTrue(tfaResult.Result, "executor didn't process TagFileProcessingError");
    }
  }
}

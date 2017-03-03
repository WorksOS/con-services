using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using System;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.TagFileAuth.Service.WebApiModels.Executors;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using Microsoft.Extensions.Logging;
using Repositories;

namespace VSS.TagFileAuth.Service.WebApiTests.Executors
{
  [TestClass]
  public class TagFileProcessingErrorExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorNoValidInput()
    {
      TagFileProcessingErrorRequest tagFileProcessingErrorRequest = new TagFileProcessingErrorRequest();
      TagFileProcessingErrorResult tagFileProcessingErrorResult = new TagFileProcessingErrorResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsFalse(result.result, "executor processed TagFileProcessingError");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorValidInputWithError()
    {
      TagFileProcessingErrorRequest TagFileProcessingErrorRequest = TagFileProcessingErrorRequest.HelpSample;
      TagFileProcessingErrorResult TagFileProcessingErrorResult = new TagFileProcessingErrorResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(TagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsFalse(result.result, "executor didn't process TagFileProcessingError without error");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorValidInputWithoutError()
    {
      TagFileProcessingErrorRequest TagFileProcessingErrorRequest = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(123, "Who Cares.tag", TagFileErrorsEnum.None);
      TagFileProcessingErrorResult TagFileProcessingErrorResult = new TagFileProcessingErrorResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(TagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.result, "executor didn't process TagFileProcessingError with error");
    }

    [TestMethod]
    public void CanCallGetTagFileProcessingErrorExecutorWithLegacyAssetId()
    {
      long legacyAssetID = 46534636436;
      string tagFileName = "Whatever";
      TagFileErrorsEnum error = TagFileErrorsEnum.None;
      var eventkeyDate = DateTime.UtcNow;
      TagFileProcessingErrorRequest TagFileProcessingErrorRequest = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(legacyAssetID, tagFileName, error);

      TagFileProcessingErrorResult TagFileProcessingErrorResult = new TagFileProcessingErrorResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(TagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.result, "executor didn't process TagFileProcessingError");
    }
  }
}

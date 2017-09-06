using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests.Executors
{
  [TestClass]
  public class TagFileProcessingErrorExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorNoValidInput()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(-1, "Machine Name--whatever --161230235959", 0);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var tfaResult = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(tfaResult, "executor returned nothing");
      Assert.IsFalse(tfaResult.Result, "executor processed TagFileProcessingError");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorValidInputWithError()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(345345, "Machine Name--whatever --161230235959", -2);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var tfaResult = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(tfaResult, "executor returned nothing");
      Assert.IsTrue(tfaResult.Result, "executor process TagFileProcessingError without error");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorValidInputWithoutError()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(123, "Machine Name--whatever --161230235959", 3);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var tfaResult = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(tfaResult, "executor returned nothing");
      Assert.IsTrue(tfaResult.Result, "executor didn't process TagFileProcessingError with error");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorValidInput2WithoutError()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(123, "Machine Name--whatever --161230235959", 2);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var tfaResult = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(tfaResult, "executor returned nothing");
      Assert.IsTrue(tfaResult.Result, "executor didn't process TagFileProcessingError with error");
    }

    [TestMethod]
    public void CanCallGetTagFileProcessingErrorExecutorWithLegacyAssetId()
    {
      long legacyAssetID = 46534636436;
      string tagFileName = "Machine Name--whatever --161230235959";
      TagFileErrorsEnum error = TagFileErrorsEnum.CoordConversion_Failure;
      var eventkeyDate = DateTime.UtcNow;
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(legacyAssetID, tagFileName, (int)error);

      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var tfaResult = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorExecutorTests>()).Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;
      Assert.IsNotNull(tfaResult, "executor returned nothing");
      Assert.IsTrue(tfaResult.Result, "executor didn't process TagFileProcessingError");
    }
  }
}

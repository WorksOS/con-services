using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.MasterData.Repositories;
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
    public void CanCallTagFileProcessingErrorExecutorNoValidInput()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(-1, "Machine Name--whatever --161230235959", 0);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>());
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsFalse(result.Result, "executor processed TagFileProcessingError");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorValidInputWithError()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(345345, "Machine Name--whatever --161230235959", -2);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>());
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor process TagFileProcessingError without error");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorValidInputWithoutError()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(123, "Machine Name--whatever --161230235959", 3);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>());
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError with error");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorExecutorValidInput2WithoutError()
    {
      TagFileProcessingErrorV1Request tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(123, "Machine Name--whatever --161230235959", 2);
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>());
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError with error");
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

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(factory, loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>());
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError");
    }
  }
}

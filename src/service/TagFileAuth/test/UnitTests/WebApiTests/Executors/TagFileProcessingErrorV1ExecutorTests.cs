using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;

namespace WebApiTests.Executors
{
  [TestClass]
  public class TagFileProcessingErrorV1ExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallTagFileProcessingErrorV1Executor_NoValidInput()
    {
      var tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(-1, "Machine Name--whatever --161230235959", 0);
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>(), ConfigStore,
        authorization.Object, cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsFalse(result.Result, "executor processed TagFileProcessingError");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorV1Executor_ValidInputWithError()
    {
      var tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(345345, "Machine Name--whatever --161230235959", -2);
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>(), ConfigStore,
        authorization.Object, cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor process TagFileProcessingError without error");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorV1Executor_ValidInputWithoutError()
    {
      var tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(123, "Machine Name--whatever --161230235959", 3);
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>(), ConfigStore,
        authorization.Object, cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError with error");
    }

    [TestMethod]
    public void CanCallTagFileProcessingErrorV1Executor_ValidInput2WithoutError()
    {
      var tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(123, "Machine Name--whatever --161230235959", 2);
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>(), ConfigStore,
        authorization.Object, cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError with error");
    }

    [TestMethod]
    public void CanCallGetTagFileProcessingErrorV1Executor_WithShortRaptorAssetId()
    {
      long shortRaptorAssetID = 46534636436;
      string tagFileName = "Machine Name--whatever --161230235959";
      var error = TagFileErrorsEnum.CoordConversion_Failure;
      var tagFileProcessingErrorRequest = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(shortRaptorAssetID, tagFileName, (int)error);

      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(loggerFactory.CreateLogger<TagFileProcessingErrorV1ExecutorTests>(), ConfigStore,
        authorization.Object, cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);
      var result = executor.Process(tagFileProcessingErrorRequest) as TagFileProcessingErrorResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsTrue(result.Result, "executor didn't process TagFileProcessingError");
    }
  }
}

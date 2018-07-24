using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace ExecutorTests
{
  [TestClass]
  public class TagFileProcessingErrorExecutorTests : ExecutorTestData
  {
    [TestMethod]
    // note that this may not work on windows container (logging= null error)
    public void TagFileProcessingErrorV1Executor()
    {
      TagFileProcessingErrorV1Request request =
        TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(1, "Data from my dozer",
          (int) TagFileErrorsEnum.ProjectID_NoMatchingArea);
      request.Validate();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(logger, configStore,
          assetRepo, deviceRepo, customerRepo,
          projectRepo, subscriptionRepo,
          producer, kafkaTopicName);
      var result =  executor.Process(request) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
    }

    [TestMethod]
    public async Task TagFileProcessingErrorV2ExecutorAsync()
    {
      string tccOrgId = Guid.NewGuid().ToString();
      int error = 1;
      string tagFileName = "Machine Name--whatever --161230235959";
      var request = TagFileProcessingErrorV2Request.CreateTagFileProcessingErrorRequest
      (tccOrgId, null, null,
        error, tagFileName,
        null, 0);
      request.Validate();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(logger, configStore,
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo,
        producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
    }
  }
}
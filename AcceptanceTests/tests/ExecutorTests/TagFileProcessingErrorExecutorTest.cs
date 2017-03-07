using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Executors;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using RepositoryTests;

namespace ExecutorTests
{
  [TestClass]
  public class TagFileProcessingErrorExecutorTests : ExecutorTestData
  {
    [TestMethod]
    public void TagFileProcessingErrorExecutor()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(1, "Data from my dozer", TagFileErrorsEnum.ProjectID_NoMatchingArea);
      request.Validate();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, logger).Process(request) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.result, "unsuccessful");
    }
  }
}

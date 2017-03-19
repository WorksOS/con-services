using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests;
using WebApiModels.Enums;
using WebApiModels.Executors;
using WebApiModels.Models;
using WebApiModels.ResultHandling;

namespace ExecutorTests
{
  [TestClass]
  public class TagFileProcessingErrorExecutorTests : ExecutorTestData
  {
    [TestMethod]
    public void TagFileProcessingErrorExecutor()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(1, "Data from my dozer", (int) TagFileErrorsEnum.ProjectID_NoMatchingArea);
      request.Validate();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, logger).Process(request) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.result, "unsuccessful");
    }
  }
}

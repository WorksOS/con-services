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
    public void TagFileProcessingErrorExecutor()
    {
      TagFileProcessingErrorV1Request request = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(1, "Data from my dozer", (int) TagFileErrorsEnum.ProjectID_NoMatchingArea);
      request.Validate();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(Factory, Logger).Process(request) as TagFileProcessingErrorResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
    }
  }
}
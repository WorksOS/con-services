using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using TestUtility;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests
{
  [TestClass]
  public class TagFileProcessingTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    public void NoValidCells_InValidPosition()
    {
      msg.Title("TagfileError Test 1", "TagFile Error NoValidCells InValidPosition");
      var ts = new TestSupport {IsPublishToKafka = false};
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589390", TagFileErrorsEnum.NoValidCells_InValidPosition);
      Assert.AreEqual(true, actualResult.Result , "result of request doesn't match expected");
    }

    [TestMethod]
    public void NoValidCells_OnGroundFlagNotSet()
    {
      msg.Title("TagfileError Test 2", "TagFileError NoValidCells OnGroundFlag NotSet");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589391", TagFileErrorsEnum.NoValidCells_OnGroundFlagNotSet);
      Assert.AreEqual(true, actualResult.Result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void ProjectID_InvalidLLHNEPosition()
    {
      msg.Title("TagfileError Test 3", "TagFileError ProjectID Invalid LLH NE Position");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589392", TagFileErrorsEnum.ProjectID_InvalidLLHNEPosition);
      Assert.AreEqual(true, actualResult.Result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void ProjectID_MultipleProjects()
    {
      msg.Title("TagfileError Test 4", "TagFileError ProjectID MultipleProjects");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589393", TagFileErrorsEnum.ProjectID_MultipleProjects);
      Assert.AreEqual(true, actualResult.Result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void ProjectID_NoMatchingArea()
    {
      msg.Title("TagfileError Test 5", "TagFileError ProjectID No Matching Area");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589394", TagFileErrorsEnum.ProjectID_NoMatchingArea);
      Assert.AreEqual(true, actualResult.Result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void ProjectID_NoMatchingDateTime()
    {
      msg.Title("TagfileError Test 6", "TagFileError ProjectID NoMatching DateTime");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589395", TagFileErrorsEnum.ProjectID_NoMatchingDateTime);
      Assert.AreEqual(true, actualResult.Result, "result of request doesn't match expected");
    }
    [TestMethod]
    public void UnknownCell()
    {
      msg.Title("TagfileError Test 7", "TagFileError UnknownCell");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589396", TagFileErrorsEnum.UnknownCell);
      Assert.AreEqual(true, actualResult.Result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void UnknownProject()
    {
      msg.Title("TagfileError Test 8", "TagFileError UnknownProject");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589397", TagFileErrorsEnum.UnknownProject);
      Assert.AreEqual(true, actualResult.Result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void CoordConversion_Failure()
    {
      msg.Title("TagfileError Test 13", "TagFileError CoordConversion Failure");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589398", TagFileErrorsEnum.CoordConversion_Failure);
      Assert.AreEqual(true, actualResult.Result, "CoordConversion_Failure");
    }

    private TagFileProcessingErrorResult CallWebApiGetTagFileProcessingErrorResult(TestSupport ts,long assetId, string tagFileName,TagFileErrorsEnum errorNum)
    {
      Thread.Sleep(500);
      var request = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(assetId,tagFileName, (int) errorNum);
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/notification/tagFileProcessingError";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      var actualResult = JsonConvert.DeserializeObject<TagFileProcessingErrorResult>(response, ts.jsonSettings);
      return actualResult;
    }
  }
}

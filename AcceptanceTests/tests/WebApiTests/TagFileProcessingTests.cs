using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

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
      Assert.AreEqual(true, actualResult.result , "result of request doesn't match expected");
    }

    [TestMethod]
    public void NoValidCells_OnGroundFlagNotSet()
    {
      msg.Title("TagfileError Test 2", "TagFileError NoValidCells OnGroundFlag NotSet");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589391", TagFileErrorsEnum.NoValidCells_OnGroundFlagNotSet);
      Assert.AreEqual(true, actualResult.result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void ProjectID_InvalidLLHNEPosition()
    {
      msg.Title("TagfileError Test 3", "TagFileError ProjectID Invalid LLH NE Position");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589392", TagFileErrorsEnum.ProjectID_InvalidLLHNEPosition);
      Assert.AreEqual(true, actualResult.result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void ProjectID_MultipleProjects()
    {
      msg.Title("TagfileError Test 4", "TagFileError ProjectID MultipleProjects");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589393", TagFileErrorsEnum.ProjectID_MultipleProjects);
      Assert.AreEqual(true, actualResult.result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void ProjectID_NoMatchingArea()
    {
      msg.Title("TagfileError Test 5", "TagFileError ProjectID No Matching Area");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589394", TagFileErrorsEnum.ProjectID_NoMatchingArea);
      Assert.AreEqual(true, actualResult.result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void ProjectID_NoMatchingDateTime()
    {
      msg.Title("TagfileError Test 6", "TagFileError ProjectID NoMatching DateTime");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589395", TagFileErrorsEnum.ProjectID_NoMatchingDateTime);
      Assert.AreEqual(true, actualResult.result, "result of request doesn't match expected");
    }
    [TestMethod]
    public void UnknownCell()
    {
      msg.Title("TagfileError Test 7", "TagFileError UnknownCell");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589396", TagFileErrorsEnum.UnknownCell);
      Assert.AreEqual(true, actualResult.result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void UnknownProject()
    {
      msg.Title("TagfileError Test 8", "TagFileError UnknownProject");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589397", TagFileErrorsEnum.UnknownProject);
      Assert.AreEqual(true, actualResult.result, "result of request doesn't match expected");
    }

    [TestMethod]
    public void UnknownErrorNoTagFileName()
    {
      msg.Title("TagfileError Test 9", "TagFileError Unknown No Tagfile Name");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "", TagFileErrorsEnum.None);
      Assert.AreEqual(true, actualResult.result, "Unknown error No Tagfile Name");
    }

    [TestMethod]
    public void UnknownError1()
    {
      msg.Title("TagfileError Test 10", "TagFileError Unknown error1");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589398", TagFileErrorsEnum.None);
      Assert.AreEqual(true, actualResult.result, "Unknown error1");
    }

    [TestMethod]
    public void UnknownError2()
    {
      msg.Title("TagfileError Test 11", "TagFileError Unknown error2");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "", TagFileErrorsEnum.None);
      Assert.AreEqual(true, actualResult.result, "Unknown error2");
    }

    [TestMethod]
    public void UnknownError3()
    {
      msg.Title("TagfileError Test 12", "TagFileError Unknown error3");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, -1, "", TagFileErrorsEnum.None);
      Assert.AreEqual(true, actualResult.result, "Unknown error3");
    }

    [TestMethod]
    public void CoordConversion_Failure()
    {
      msg.Title("TagfileError Test 13", "TagFileError CoordConversion Failure");
      var ts = new TestSupport { IsPublishToKafka = false };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589398", TagFileErrorsEnum.CoordConversion_Failure);
      Assert.AreEqual(true, actualResult.result, "CoordConversion_Failure");
    }

    private TagFileProcessingErrorResult CallWebApiGetTagFileProcessingErrorResult(TestSupport ts,long assetId, string tagFileName,TagFileErrorsEnum errorNum)
    {
      var request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(assetId,tagFileName,errorNum);
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/notification/tagFileProcessingError";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      msg.DisplayWebApi(method, uri, response, requestJson);
      var actualResult = JsonConvert.DeserializeObject<TagFileProcessingErrorResult>(response, ts.jsonSettings);
      return actualResult;
    }
  }
}

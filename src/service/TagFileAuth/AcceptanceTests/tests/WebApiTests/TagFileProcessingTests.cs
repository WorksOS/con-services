using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using TestUtility;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using Xunit;

namespace WebApiTests
{
  public class TagFileProcessingTests
  {
    private readonly Msg msg = new Msg();

    [Fact]
    public void Notification_NoValidCells_InValidPosition()
    {
      msg.Title("TagfileError Test 1", "TagFile Error NoValidCells InValidPosition");
      var ts = new TestSupport { IsPublishToWebApi = true};
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589390", TagFileErrorsEnum.NoValidCells_InValidPosition);
      Assert.True(actualResult.Result);
    }

    [Fact]
    public void Notification_NoValidCells_OnGroundFlagNotSet()
    {
      msg.Title("TagfileError Test 2", "TagFileError NoValidCells OnGroundFlag NotSet");
      var ts = new TestSupport { IsPublishToWebApi = true };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589391", TagFileErrorsEnum.NoValidCells_OnGroundFlagNotSet);
      Assert.True(actualResult.Result);
    }

    [Fact]
    public void Notification_ProjectID_InvalidLLHNEPosition()
    {
      msg.Title("TagfileError Test 3", "TagFileError ProjectID Invalid LLH NE Position");
      var ts = new TestSupport { IsPublishToWebApi = true };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589392", TagFileErrorsEnum.ProjectID_InvalidLLHNEPosition);
      Assert.True(actualResult.Result);
    }

    [Fact]
    public void Notification_ProjectID_MultipleProjects()
    {
      msg.Title("TagfileError Test 4", "TagFileError ProjectID MultipleProjects");
      var ts = new TestSupport { IsPublishToWebApi = true };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589393", TagFileErrorsEnum.ProjectID_MultipleProjects);
      Assert.True(actualResult.Result);
    }

    [Fact]
    public void Notification_ProjectID_NoMatchingArea()
    {
      msg.Title("TagfileError Test 5", "TagFileError ProjectID No Matching Area");
      var ts = new TestSupport { IsPublishToWebApi = true };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589394", TagFileErrorsEnum.ProjectID_NoMatchingArea);
      Assert.True(actualResult.Result);
    }

    [Fact]
    public void Notification_ProjectID_NoMatchingDateTime()
    {
      msg.Title("TagfileError Test 6", "TagFileError ProjectID NoMatching DateTime");
      var ts = new TestSupport { IsPublishToWebApi = true };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589395", TagFileErrorsEnum.ProjectID_NoMatchingDateTime);
      Assert.True(actualResult.Result);
    }

    [Fact]
    public void Notification_UnknownCell()
    {
      msg.Title("TagfileError Test 7", "TagFileError UnknownCell");
      var ts = new TestSupport { IsPublishToWebApi = true };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589396", TagFileErrorsEnum.UnknownCell);
      Assert.True(actualResult.Result);
    }

    [Fact]
    public void Notification_UnknownProject()
    {
      msg.Title("TagfileError Test 8", "TagFileError UnknownProject");
      var ts = new TestSupport { IsPublishToWebApi = true };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589397", TagFileErrorsEnum.UnknownProject);
      Assert.True(actualResult.Result);
    }

    [Fact]
    public void Notification_CoordConversion_Failure()
    {
      msg.Title("TagfileError Test 13", "TagFileError CoordConversion Failure");
      var ts = new TestSupport { IsPublishToWebApi = true };
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "2129J001DV--422c--150707_5408589398", TagFileErrorsEnum.CoordConversion_Failure);
      Assert.True(actualResult.Result);
    }

    private TagFileProcessingErrorResult CallWebApiGetTagFileProcessingErrorResult(TestSupport ts,long assetId, string tagFileName,TagFileErrorsEnum errorNum)
    {
      Thread.Sleep(500);
      var request = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(assetId,tagFileName, (int) errorNum);
      var requestJson = JsonConvert.SerializeObject(request, ts.JsonSettings);
      var restClient = new RestClientUtil();
      var uri = ts.GetBaseUri() + "api/v1/notification/tagFileProcessingError";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      var actualResult = JsonConvert.DeserializeObject<TagFileProcessingErrorResult>(response, ts.JsonSettings);
      return actualResult;
    }
  }
}

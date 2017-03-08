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
    public void InvalidPosition()
    {
      msg.Title("Asset WebTest 1", "Inject device type3 with assetId (radio serial), call webAPI to get asset Id");
      var ts = new TestSupport {IsPublishToKafka = false};
      var actualResult = CallWebApiGetTagFileProcessingErrorResult(ts, 1234567890, "tagfileName",TagFileErrorsEnum.NoValidCells_InValidPosition);
      Assert.AreEqual(true, actualResult.result , " result of request doesn't match expected");
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

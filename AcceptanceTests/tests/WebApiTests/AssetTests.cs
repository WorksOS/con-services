using System;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;


namespace WebApiTests
{
  [TestClass]
  public class AssetTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    public void InjectAnAssetIntoDatabaseAndRetrieveAssetIdFromWebapi()
    {
      msg.Title("Asset Test 1", "Inject an asset into database and retrieve AssetId from web api");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var eventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name      | MakeCode | SerialNumber | Model | IconKey | AssetType  | ",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | AssetT1   | CAT      | XAT1         | 345D  | 10      | Excavators | "};
      ts.PublishEventCollection(eventArray);
      var request = GetAssetIdRequest.CreateGetAssetIdRequest(legacyAssetId, 0, "123456");
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings );
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/asset/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri,method, requestJson);
      msg.DisplayWebApi(method, uri, response, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response,ts.jsonSettings);
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");

    }
  }
}

using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using TestUtility;

namespace IntegrationTests
{
  [TestClass]
  public class AssetIntTests
  {
    [TestMethod]
    public void InjectAssetDeviceEventsAndCallWebApiGetId()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      var legacyAssetId = ts.SetLegacyAssetId();
      msg.Title("Asset Int Test 1", "Inject Asset Device Events And Call WebApi GetId");
      var eventArray = new[] {
         "| EventType                 | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | AssetUID      | AssetName | Make | SerialNumber | Model | IconKey | AssetType  |",
        $"| CreateDeviceEvent         | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | 4G           |               |           |      |              |       |         |            |",
        $"| CreateAssetEvent          | 0d+09:00:00 |                    |             |            |             |              | {ts.AssetUid} | AssetInt1 | CAT  | XAT1         | 374D  | 10      | Excavators |",
        $"| AssociateDeviceAssetEvent | 0d+09:10:00 |                    |             |            | {deviceUid} |              | {ts.AssetUid} |           |      |              |       |         |            |"
      };

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseRecordCount("Asset", "AssetUID", 1, new Guid(ts.AssetUid));

      var request = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 6, deviceUid.ToString());
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

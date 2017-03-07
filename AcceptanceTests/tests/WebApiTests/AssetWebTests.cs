using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;


namespace WebApiTests
{
  [TestClass]
  public class AssetWebTests
  {
    [TestMethod]
    public void Createadevicetype3Andassetwithradioserial()
    {
      var msg = new Msg();
      msg.Title("Asset WebTest 1", "Inject device type3 with assetId (radio serial), call webAPI to get asset Id");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var eventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name      | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | AssetT1   | CAT      | XAT1         | 345D  | 10      | Excavators |                   |"};
      ts.PublishEventCollection(eventArray);

      var deviceUid = Guid.NewGuid();
      var deviceEventArray = new[] {
         "| TableName | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeregisteredUTC | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | MainboardSoftwareVersion | ModuleType | RadioFirmwarePartNumber |",
        $"| Device    | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | 0d+09:00:00     | {deviceUid} | CDMA         | 1.23                      | 3.54                     | modtyp     | 88                      |"};
      ts.PublishEventCollection(deviceEventArray);
      var assetdeviceEventArray = new[] {
         "| TableName   | EventDate   | fk_AssetUID   | fk_DeviceUID |",
        $"| AssetDevice | 0d+09:10:00 | {ts.AssetUid} | {deviceUid}  |"};
      ts.PublishEventCollection(assetdeviceEventArray);

      var request = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 3, deviceUid.ToString());
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings );
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/asset/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri,method, requestJson);
      msg.DisplayWebApi(method, uri, response, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response,ts.jsonSettings);
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
    }

    [TestMethod]
    public void CallwebApItogetassetIdfornonexistentradioserial()
    {
      var msg = new Msg();
      msg.Title("Asset WebTest 2", "Call webAPI to get asset Id for non existent rado serial");
      var ts = new TestSupport { IsPublishToKafka = false };
      var deviceUid = Guid.NewGuid();
      var request = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 3, deviceUid.ToString());
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings );
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/asset/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri,method, requestJson);
      msg.DisplayWebApi(method, uri, response, requestJson);
     // var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response,ts.jsonSettings);
     // Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");

    }

    [TestMethod]
    public void Createadevicetype3Andassetwithradioserialwithownercustomerandman3Dsubscription()
    {
      var msg = new Msg();
      msg.Title("Asset WebTest 3", "Inject Man3D PM sub, device type3 with assetId(radio serial) WITH OWNERCUSTOMER, call webAPI to get asset Id");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyAssetId = ts.SetLegacyAssetId();
      var customerUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var endDate = new DateTime(9999, 12, 31);
      var deviceUid = Guid.NewGuid();
      // Write events to database 
      var eventsArray = new[] {
         "| TableName    | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate                | EndDate        | ",
        $"| Customer     | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                | 15               |                          |                | ",
        $"| Subscription | 0d+09:00:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 15               | {ts.FirstEventDate.Date} | {endDate.Date} | "};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name      | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | AssetT1   | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName   | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | fk_AssetUID   | fk_DeviceUID |",
      $"| Device      | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         |               |              |",
      $"| AssetDevice | 0d+09:20:00 |                    |             |            |             |              | {ts.AssetUid} | {deviceUid}  |"};
      ts.PublishEventCollection(deviceEventArray);
      //Call Web api
      var request = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 3, deviceUid.ToString());
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/asset/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      msg.DisplayWebApi(method, uri, response, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response, ts.jsonSettings);
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(15, actualResult.machineLevel, " Machine level do not match ");
    }
  }
}

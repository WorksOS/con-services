using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using TestUtility;
using TestUtility.Model.Enums;

namespace IntegrationTests
{
  [TestClass]
  public class AssetIntTests
  {
    [TestMethod]
    public void InjectAssetDeviceEventsAndCallWebApiGetId()
    {
      var msg = new Msg();
      msg.Title("Asset Int Test 1", "Inject Asset Device Events And Call WebApi GetId");
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var deviceUid = Guid.NewGuid();
      var legacyAssetId = ts.SetLegacyAssetId();
      var eventArray = new[] {
         "| EventType                 | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | AssetUID      | AssetName | Make | SerialNumber | Model | IconKey | AssetType  | LegacyAssetId |",
        $"| CreateDeviceEvent         | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | 4G           |               |           |      |              |       |         |            |               |",
        $"| CreateAssetEvent          | 0d+09:00:00 |                    |             |            |             |              | {ts.AssetUid} | AssetInt1 | CAT  | XAT1         | 374D  | 10      | Excavators |{legacyAssetId}|",
        $"| AssociateDeviceAssetEvent | 0d+09:10:00 |                    |             |            | {deviceUid} |              | {ts.AssetUid} |           |      |              |       |         |            |               |"
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

    [TestMethod]
    public void InjectProjectCustomerManual3DpmSubscriptionAssetanddevicecallwebApIwithprojectid()
    {
      var msg = new Msg();
      msg.Title("Asset Int Test 2", "Inject Asset,Device,Project and customers events. Call WebApi GetId and return machine level and asset id");
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var deviceEventArray = new[] {
       "| EventType                 | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | AssetUID      | AssetName | Make | SerialNumber | Model | IconKey | AssetType  | LegacyAssetId   | OwningCustomerUID |",
      $"| CreateDeviceEvent         | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | 4G           |               |           |      |              |       |         |            |                 |                   |",
      $"| CreateAssetEvent          | 0d+09:00:00 |                    |             |            |             |              | {ts.AssetUid} | AssetInt1 | CAT  | XAT1         | 374D  | 10      | Excavators | {legacyAssetId} | {customerUid}     |",
      $"| AssociateDeviceAssetEvent | 0d+09:10:00 |                    |             |            | {deviceUid} |              | {ts.AssetUid} |           |      |              |       |         |            |                 |                   |"
      };
      ts.PublishEventCollection(deviceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseRecordCount("Asset", "AssetUID", 1, new Guid(ts.AssetUid));

      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectID         | ProjectUID    | ProjectName   | ProjectType                     | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |",
      $"| CreateProjectEvent | 1d+09:00:00 | {legacyProjectId} | {projectUid}  | AssetIntTest2 | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWkt} |"};

      ts.PublishEventCollection(projectEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid);
      var endDt = new DateTime(9999, 12, 31);
      var custEventArray = new[] { 
       "| EventType                         | EventDate   | CustomerName | CustomerType | CustomerUID   | EffectiveDate       | ProjectUID   | SubscriptionUID   | StartDate          | EndDate | SubscriptionType   |",
      $"| CreateCustomerEvent               | 0d+09:00:00 | CustName     | Customer     | {customerUid} |                     |              |                   |                    |         |                    |",
      $"| CreateProjectSubscriptionEvent    | 0d+09:00:00 |              |              |               |                     |              | {subscriptionUid} |{ts.FirstEventDate} | {endDt} | Project Monitoring |",
      $"| AssociateProjectSubscriptionEvent | 0d+09:00:00 |              |              |               | {ts.FirstEventDate} | {projectUid} | {subscriptionUid} |                    |         |                    |"};

      ts.PublishEventCollection(custEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);
      var request = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 6, deviceUid.ToString());
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/asset/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      msg.DisplayWebApi(method, uri, response, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response, ts.jsonSettings);

      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(18, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.result, " result of request doesn't match expected");
    }
  }
}

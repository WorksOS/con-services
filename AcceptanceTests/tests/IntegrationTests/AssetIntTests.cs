using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using TestUtility;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace IntegrationTests
{
  [TestClass]
  public class AssetIntTests
  {
    [TestMethod]
    public void SimpleInjectAssetDeviceEventsAndCallWebApiGetId()
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
      var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response,ts.jsonSettings);
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
    }

    [TestMethod]
    public void Manual3DpmSubscription_AssetProjectAndDevice()
    {
      var msg = new Msg();
      msg.Title("Asset Int Test 2", "Inject Asset,Device,Project and customers events with manual 3D subscription. Call WebApi GetId and return machine level and asset id");

      /*
       CultureInfo cultureInfo = new CultureInfo("en-US");
       var notFormated = new DateTime(9999, 12, 31);
       var formated = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
       var formatedWithCulture = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd").ToString(cultureInfo);
       Console.WriteLine($"Manual3DpmSubscription_AssetProjectAndDevice 0 jeannieTest: notFormated: {notFormated}");
       Console.WriteLine($"Manual3DpmSubscription_AssetProjectAndDevice 0 jeannieTest: formated: {formated}");
       Console.WriteLine($"Manual3DpmSubscription_AssetProjectAndDevice 0 jeannieTest: formatedWithCulture: {formatedWithCulture}");
      */

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
      $"| CreateAssetEvent          | 0d+09:05:00 |                    |             |            |             |              | {ts.AssetUid} | AssetInt2 | CAT  | XAT1         | 374D  | 10      | Excavators | {legacyAssetId} | {customerUid}     |",
      $"| AssociateDeviceAssetEvent | 0d+09:10:00 |                    |             |            | {deviceUid} |              | {ts.AssetUid} |           |      |              |       |         |            |                 |                   |"
      };
      ts.PublishEventCollection(deviceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseRecordCount("Asset", "AssetUID", 1, new Guid(ts.AssetUid));

      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectID         | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |",
      $"| CreateProjectEvent | 1d+09:00:00 | {legacyProjectId} | {projectUid}  | AssetIntTest2 | {ProjectType.Standard} | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWkt} |"};

      ts.PublishEventCollection(projectEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid);
      var endDt = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var custEventArray = new[] { 
       "| EventType                         | EventDate   | CustomerName | CustomerType | CustomerUID   | ProjectUID   | SubscriptionUID   | StartDate          | EndDate | SubscriptionType             |",
      $"| CreateCustomerEvent               | 0d+09:00:00 | CustName     | Customer     | {customerUid} |              |                   |                    |         |                              |",
      $"| CreateCustomerSubscriptionEvent   | 0d+09:01:00 |              |              | {customerUid} |              | {subscriptionUid} |{ts.FirstEventDate} | {endDt} | Manual 3D Project Monitoring |",
      $"| AssociateProjectSubscriptionEvent | 0d+09:02:00 |              |              |               | {projectUid} | {subscriptionUid} |                    |         |                              |"};

      ts.PublishEventCollection(custEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectSubscription", "fk_SubscriptionUID", 1, subscriptionUid);
      Thread.Sleep(2000);
      var request = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 6, deviceUid.ToString());
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/asset/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response, ts.jsonSettings);
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(18, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");

      // Try again project id as the parameter as well
      Console.WriteLine("---------------------- 2nd call to web api -----------------------");
      request = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 6, deviceUid.ToString());
      requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      response = restClient.DoHttpRequest(uri, method, requestJson);
      actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response, ts.jsonSettings);

      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(18, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void ThreeDPMSubscription_AssetProjectAndDevice()
    {
      var msg = new Msg();
      msg.Title("Asset Int Test 3", "Inject Asset,Device,Project and customers events with 3D subscription. Call WebApi GetId and return machine level and asset id");
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
      $"| CreateAssetEvent          | 0d+09:05:00 |                    |             |            |             |              | {ts.AssetUid} | AssetInt3 | CAT  | XAT1         | 374D  | 10      | Excavators | {legacyAssetId} | {customerUid}     |",
      $"| AssociateDeviceAssetEvent | 0d+09:10:00 |                    |             |            | {deviceUid} |              | {ts.AssetUid} |           |      |              |       |         |            |                 |                   |"
      };
      ts.PublishEventCollection(deviceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseRecordCount("Asset", "AssetUID", 1, new Guid(ts.AssetUid));

      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectID         | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |",
      $"| CreateProjectEvent | 1d+09:00:00 | {legacyProjectId} | {projectUid}  | AssetIntTest3 | {ProjectType.Standard} | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWkt} |"};

      ts.PublishEventCollection(projectEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid);
      var endDt = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var custEventArray = new[] { 
       "| EventType                         | EventDate   | CustomerName | CustomerType | CustomerUID   | ProjectUID   | SubscriptionUID   | StartDate          | EndDate | SubscriptionType      | DeviceUID   | AssetUID      |",
      $"| CreateCustomerEvent               | 0d+09:00:00 | CustName     | Customer     | {customerUid} |              |                   |                    |         |                       |             |               |",
      $"| CreateAssetSubscriptionEvent      | 0d+09:01:00 |              |              | {customerUid} |              | {subscriptionUid} |{ts.FirstEventDate} | {endDt} | 3D Project Monitoring | {deviceUid} | {ts.AssetUid} |"};

      ts.PublishEventCollection(custEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);
      Thread.Sleep(2000);
      var request = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 6, deviceUid.ToString());
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/asset/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response, ts.jsonSettings);

      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(16, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void LandfillSubscriptionProject()
    {
      var msg = new Msg();
      msg.Title("Asset Int Test 4", "Inject landfill project and subscription. Call WebApi GetId and return machine level and asset id");

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
      $"| CreateAssetEvent          | 0d+09:05:00 |                    |             |            |             |              | {ts.AssetUid} | AssetInt4 | CAT  | XAT1         | 374D  | 10      | Excavators | {legacyAssetId} | {customerUid}     |",
      $"| AssociateDeviceAssetEvent | 0d+09:10:00 |                    |             |            | {deviceUid} |              | {ts.AssetUid} |           |      |              |       |         |            |                 |                   |"
      };
      ts.PublishEventCollection(deviceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseRecordCount("Asset", "AssetUID", 1, new Guid(ts.AssetUid));

      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectID         | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |",
      $"| CreateProjectEvent | 1d+09:00:00 | {legacyProjectId} | {projectUid}  | AssetIntTest4 | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWkt} |"};

      ts.PublishEventCollection(projectEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid);
      var endDt = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var custEventArray = new[] { 
       "| EventType                         | EventDate   | CustomerName | CustomerType | CustomerUID   | ProjectUID   | SubscriptionUID   | StartDate          | EndDate | SubscriptionType | DeviceUID   | AssetUID      |",
      $"| CreateCustomerEvent               | 0d+09:00:00 | CustName     | Customer     | {customerUid} |              |                   |                    |         |                  |             |               |",
      $"| CreateProjectSubscriptionEvent    | 0d+09:01:00 |              |              | {customerUid} |              | {subscriptionUid} |{ts.FirstEventDate} | {endDt} | Landfill         | {deviceUid} | {ts.AssetUid} |"};

      ts.PublishEventCollection(custEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);
      Thread.Sleep(2000);
      var request = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 6, deviceUid.ToString());
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/asset/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response, ts.jsonSettings);
      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(0, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");

      // Try again project id as the parameter as well
      Console.WriteLine("---------------------- 2nd call to web api -----------------------");
      request = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 6, deviceUid.ToString());
      requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      response = restClient.DoHttpRequest(uri, method, requestJson);
      actualResult = JsonConvert.DeserializeObject<GetAssetIdResult>(response, ts.jsonSettings);

      Assert.AreEqual(legacyAssetId, actualResult.assetId, " Legacy asset id's do not match");
      Assert.AreEqual(0, actualResult.machineLevel, " Machine levels do not match ");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }
  }
}

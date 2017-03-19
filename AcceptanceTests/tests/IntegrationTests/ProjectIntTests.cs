using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using WebApiModels.ResultHandling;
using WebApiModels.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace IntegrationTests
{
  [TestClass]
  public class ProjectIntTests
  {
    [TestMethod]
    public void ThreeDPMSubscription_GetProjectId()
    {
      var msg = new Msg();
      msg.Title("Project Int Test 1", "Inject Asset,Device,Project and customer events with 3D subscription. Call projects/getId and return project id");
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceEventArray = new[] {
       "| EventType                 | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | AssetUID      | AssetName | Make | SerialNumber | Model | IconKey | AssetType  | LegacyAssetId   | OwningCustomerUID |",
      $"| CreateDeviceEvent         | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | 4G           |               |           |      |              |       |         |            |                 |                   |",
      $"| CreateAssetEvent          | 0d+09:05:00 |                    |             |            |             |              | {ts.AssetUid} | AssetInt3 | CAT  | XAT1         | 374D  | 10      | Excavators | {legacyAssetId} | {customerUid}     |",
      $"| AssociateDeviceAssetEvent | 0d+09:10:00 |                    |             |            | {deviceUid} |              | {ts.AssetUid} |           |      |              |       |         |            |                 |                   |"};
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
      var endDt = new DateTime(9999, 12, 31);
      var custEventArray = new[] {
       "| EventType                         | EventDate   | CustomerName | CustomerType | CustomerUID   | ProjectUID   | SubscriptionUID   | StartDate          | EndDate | SubscriptionType      | DeviceUID   | AssetUID      |",
      $"| CreateCustomerEvent               | 0d+09:00:00 | CustName     | Customer     | {customerUid} |              |                   |                    |         |                       |             |               |",
      $"| CreateAssetSubscriptionEvent      | 0d+09:01:00 |              |              | {customerUid} |              | {subscriptionUid} |{ts.FirstEventDate} | {endDt} | 3D Project Monitoring | {deviceUid} | {ts.AssetUid} |"};
      ts.PublishEventCollection(custEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);

      var associateEventArray = new[] {
       "| EventType                | EventDate   | ProjectUID    | CustomerUID  | ",
      $"| AssociateProjectCustomer | 0d+09:00:00 | {projectUid} | {customerUid} | "};
      ts.PublishEventCollection(associateEventArray);

      ts.IsPublishToKafka = false;
      var custTccOrg = new[] {
        "| TableName       | EventDate   | CustomerUID   | TCCOrgID |",
       $"| CustomerTccOrg  | 0d+09:00:00 | {customerUid} | {tccOrg} |"};
      ts.PublishEventCollection(custTccOrg);

      var actualResult = CallWebApiGetProjectId(ts, legacyAssetId, 38.837, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(legacyProjectId, actualResult.projectId, " Legacy asset id's do not match");
      Assert.AreEqual(true, actualResult.result, " result of request doesn't match expected");
    }

    /// <summary>
    /// Call the get project request
    /// </summary>
    /// <param name="ts">test support</param>
    /// <param name="assetid">legacy asset ID (radio serial)</param>
    /// <param name="latitude">seed position latitude value from tagfile</param>
    /// <param name="longitude">seed position longitude value from tagfile</param>
    /// <param name="timeOfPosition">from tagfile-used to check against valid Project time range.</param>
    /// <param name="tccOrgUid">UID of the TCC account the VL customer is paired with. 
    ///   Identifies which VL customer projects to search.</param>
    /// <returns></returns>
    private GetProjectIdResult CallWebApiGetProjectId(TestSupport ts,long assetid,double latitude,double longitude, DateTime timeOfPosition,string tccOrgUid)
    {
      var request = GetProjectIdRequest.CreateGetProjectIdRequest(assetid,latitude,longitude, 0, timeOfPosition,tccOrgUid);
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/project/getId";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetProjectIdResult>(response, ts.jsonSettings);
      return actualResult;
    }
  }
}

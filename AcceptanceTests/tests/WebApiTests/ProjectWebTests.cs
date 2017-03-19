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
  public class ProjectWebTests
  {
    private readonly Msg msg = new Msg();

   [TestMethod]
    public void InjectProjectCustomerManual3DpmSubscriptionAssetanddevicecallwebApIwithprojectid()
    {
      msg.Title("Project WebTest 1", "   ");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest1 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
        "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID |",
       $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |",
       $"| CustomerTccOrg  | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |",
       $"| Subscription    | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |",
       $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name           | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | AssetWebTest4  | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest1           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

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

      
  
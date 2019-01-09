using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using TestUtility;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests
{
  [TestClass]
  public class ProjectV2WebTRexEndpointTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    public void TRexEndPoint_Manual_Happy_StdPrj_PrjMan3d()
    {
      msg.Title("Project TRex WebTest 1", "ManualImport happy path: Project has Man3d and location within boundary");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var projectStartDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var projectEndDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((170 10, 190 10, 190 40, 180 40, 170 40, 170 10))";

      // Write events to database 
      var projectType = (int)ProjectType.Standard;
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name              | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate          | EndDate          | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectV2WebTest1 | {projectType}    | New Zealand Standard Time | Pacific/Auckland | {projectStartDate} | {projectEndDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);

      var serviceType = (int)ServiceTypeEnum.Manual3DProjectMonitoring;
      var subStartDate = ts.FirstEventDate.AddDays(1).ToString("yyyy-MM-dd");
      var subEndDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var eventsArray = new[] {
         "| TableName            | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate      | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer             | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |                |                |               |          |                    |",
        $"| Subscription         | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | {serviceType}    | {subStartDate} | {subEndDate}   |               |          |                    |",
        $"| CustomerProject      | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |                |                | {projectUid}  |          |                    |"};
      ts.PublishEventCollection(eventsArray);
     
      var actualResult = CallWebApiGetProjectAndAssetUids(ts, projectUid.ToString(), 0, string.Empty, string.Empty, string.Empty, 11, 171, ts.FirstEventDate.AddDays(1));
      Assert.AreEqual(projectUid.ToString(), actualResult.ProjectUid, "ProjectUid does not match");
      Assert.AreEqual(string.Empty, actualResult.AssetUid, "AssetUid does not match");
      Assert.AreEqual(0, actualResult.Code, " result code of request doesn't match expected");
    }

    [TestMethod]
    public void TRexEndPoint_Manual_Happy_StdPrj_Asset3dSub_MatchesProjectCustomer()
    {
      msg.Title("Project TRex WebTest 2", "ManualImport happy path: Assets customer has Man3d, assetOwner is == projectOwner, and location within boundary");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var projectStartDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var projectEndDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((170 10, 190 10, 190 40, 180 40, 170 40, 170 10))";

      // Write events to database 
      var projectType = (int)ProjectType.Standard;
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name              | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate          | EndDate          | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectV2WebTest1 | {projectType}    | New Zealand Standard Time | Pacific/Auckland | {projectStartDate} | {projectEndDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);

      var assetEventArray = new[] {
        "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
        $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectV2WebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);

      var deviceType = DeviceType.SNM940;
      var deviceEventArray = new[] {
         "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType   | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID |",
        $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | {deviceType} | {deviceUid} | CDMA         | ProjectV2WebTest3         |               |              |                    |",
        $"| AssetDevice       | 0d+09:20:00 |                    |             |              |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |"};
      ts.PublishEventCollection(deviceEventArray);

      var serviceType = (int)ServiceTypeEnum.ThreeDProjectMonitoring;
      var subStartDate = ts.FirstEventDate.AddDays(1).ToString("yyyy-MM-dd");
      var subEndDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var eventsArray = new[]
      {
        "| TableName          | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate      | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID | fk_AssetUID   | EffectiveDate      | ",
        $"| Customer          | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |                |                |               |          |                    |               |                    |",
        $"| Subscription      | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | {serviceType}    | {subStartDate} | {subEndDate}   |               |          |                    |               |                    |",
        $"| CustomerProject   | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |                |                | {projectUid}  |          |                    |               |                    |",
        $"| AssetSubscription | 0d+09:00:00 |               |           |                   |                   |                |                  |                |                |               |          |  {subscriptionUid} | {ts.AssetUid} | {projectStartDate} |"};
      ts.PublishEventCollection(eventsArray);

      var actualResult = CallWebApiGetProjectAndAssetUids(ts, projectUid.ToString(), (int)deviceType, deviceUid.ToString(), string.Empty, string.Empty, 11, 171, ts.FirstEventDate.AddDays(1));
      Assert.AreEqual(projectUid.ToString(), actualResult.ProjectUid, "ProjectUid does not match");
      Assert.AreEqual(ts.AssetUid, actualResult.AssetUid, "AssetUid does not match");
      Assert.AreEqual(0, actualResult.Code, " result code of request doesn't match expected");
    }

    [TestMethod]
    public void TRexEndPoint_Auto_Happy_LFPrj()
    {
      msg.Title("Project TRex WebTest 3", "AutoImport happy path: tccOrgId customer found. Project is landfill and has a landfill sub, for the tccOrg customerUid. Location within boundary");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var tccOrg = Guid.NewGuid();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccCustomerUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var projectStartDate = ts.FirstEventDate.Date.ToString("yyyy-MM-dd");
      var projectEndDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((170 10, 190 10, 190 40, 180 40, 170 40, 170 10))";

      // Write events to database 
      var projectType = (int)ProjectType.LandFill;
      var projectEventArray = new[] {
         "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name              | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate          | EndDate          | GeometryWKT   |",
        $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectV2WebTest1 | {projectType}    | New Zealand Standard Time | Pacific/Auckland | {projectStartDate} | {projectEndDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);

      var custTccOrg = new[] {
         "| TableName       | EventDate   | CustomerUID      | TCCOrgID |",
        $"| CustomerTccOrg  | 0d+09:00:00 | {tccCustomerUid} | {tccOrg} |"};
      ts.PublishEventCollection(custTccOrg);

      var serviceType = (int)ServiceTypeEnum.Landfill;
      var subStartDate = ts.FirstEventDate.AddDays(1).ToString("yyyy-MM-dd");
      var subEndDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var eventsArray = new[]
      {
         "| TableName           | EventDate   | CustomerUID      | Name         | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID   | fk_ServiceTypeID | StartDate      | EndDate        | fk_ProjectUID | fk_SubscriptionUID | EffectiveDate      |",
        $"| Customer            | 0d+09:00:00 | {customerUid}    | CustName     | 1                 |                   |                  |                  |                |                |               |                    |                    |",
        $"| Customer            | 0d+09:00:00 | {tccCustomerUid} | TccCustName  | 1                 |                   |                  |                  |                |                |               |                    |                    |",
        $"| Subscription        | 0d+09:10:00 |                  |              |                   | {subscriptionUid} | {tccCustomerUid} | {serviceType}    | {subStartDate} | {subEndDate}   |               |                    |                    |",
        $"| CustomerProject     | 0d+09:20:00 |                  |              |                   |                   | {tccCustomerUid} |                  |                |                | {projectUid}  |                    |                    |",
        $"| ProjectSubscription | 0d+09:00:00 |                  |              |                   |                   |                  |                  |                |                | {projectUid}  |  {subscriptionUid} | {projectStartDate} |"};
      ts.PublishEventCollection(eventsArray);

      var actualResult = CallWebApiGetProjectAndAssetUids(ts, string.Empty, 0, string.Empty, string.Empty, tccOrg.ToString(), 11, 171, ts.FirstEventDate.AddDays(1));
      Assert.AreEqual(projectUid.ToString(), actualResult.ProjectUid, "ProjectUid does not match");
      Assert.AreEqual(string.Empty, actualResult.AssetUid, "AssetUid does not match");
      Assert.AreEqual(0, actualResult.Code, " result code of request doesn't match expected");
    }

    /// <summary>
    /// Call the get project and asset request
    /// </summary>
    /// <param name="ts">test support</param>
    /// <param name="projectUid"></param>
    /// <param name="deviceType"></param>
    /// <param name="radioSerial"></param>
    /// <param name="tccOrgUid"></param>
    /// <param name="latitude">seed position latitude value from tagfile</param>
    /// <param name="longitude">seed position longitude value from tagfile</param>
    /// <param name="timeOfPosition">from tagfile-used to check against valid Project time range.</param>
    /// <param name="statusCode"></param>
    /// <returns>The project Uid result</returns>
    private GetProjectAndAssetUidsResult CallWebApiGetProjectAndAssetUids(TestSupport ts, string projectUid, int deviceType, string radioSerial, string ec520Serial, string tccOrgUid, double latitude, double longitude, DateTime timeOfPosition, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      Thread.Sleep(500);
      var request = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, deviceType, radioSerial, ec520Serial, tccOrgUid, latitude, longitude, timeOfPosition);
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v2/project/getUids";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson, statusCode);
      var actualResult = JsonConvert.DeserializeObject<GetProjectAndAssetUidsResult>(response, ts.jsonSettings);
      return actualResult;
    }
  }
}

      
  
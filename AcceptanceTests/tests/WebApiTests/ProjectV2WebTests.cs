using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using TestUtility;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests
{
  [TestClass]
  public class ProjectV2WebTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    public void AssetDevice_NoSubsOrProjects()
    {
      msg.Title("Project WebTest 1", "No subscription and no project. Get no project Uid for ProjectUid request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyAssetId = ts.SetLegacyAssetId();
      var deviceUid = Guid.NewGuid();
      // Write events to database 
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectV2WebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators |                   |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | ProjectV2WebTest1           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectUid(ts, 6, deviceUid.ToString(), 38.837, -121.348, ts.FirstEventDate.AddDays(1));
      Assert.AreEqual("", actualResult.ProjectUid, "ProjectUid does not match");
      Assert.AreEqual(3029, actualResult.Code, " result code of request doesn't match expected");
    }

    [TestMethod]
    public void AssetDevice_3dPMSubAndProject()
    {
      msg.Title("Project WebTest 2", "3D project monitoring subscription and a standard project. Get the projectUid for ProjectUid request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectV2WebTest1 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
        "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID |",
       $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |",
       $"| Subscription    | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |",
       $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectV2WebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | ProjectV2WebTest2           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectUid(ts, 6, deviceUid.ToString(), 38.837, -121.348, ts.FirstEventDate.AddDays(1));
      Assert.AreEqual(projectUid.ToString(), actualResult.ProjectUid, "ProjectUid does not match");
      Assert.AreEqual(0, actualResult.Code, " result code of request doesn't match expected");
    }

    [TestMethod]
    public void AssetDevice_3dPMSubAndProject_UnsupportedDeviceType()
    {
      msg.Title("Project WebTest 2", "3D project monitoring subscription and a standard project. Get the projectUid for ProjectUid request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectV2WebTest1 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
        "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID |",
       $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |",
       $"| Subscription    | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |",
       $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectV2WebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series521  | {deviceUid} | CDMA         | ProjectV2WebTest2           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectUid(ts, 3, deviceUid.ToString(), 38.837, -121.348, ts.FirstEventDate.AddDays(1), HttpStatusCode.BadRequest);
      Assert.AreEqual(String.Empty, actualResult.ProjectUid, "ProjectUid does not match");
      Assert.AreEqual(3030, actualResult.Code, " result code of request doesn't match expected");
    }

    [TestMethod]
    public void AssetDevice_3dPMSubAndProject_LatLongOutSideBoundary()
    {
      msg.Title("Project WebTest 3", "3D project monitoring subscription for standard project. Call web api with lat long outside boundary");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectV2WebTest1 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
        "| TableName       | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID |",
       $"| Customer        | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |",
       $"| Subscription    | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |",
       $"| CustomerProject | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectV2WebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | ProjectV2WebTest3           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectUid(ts, 6, deviceUid.ToString(), 40.837, -121.348, ts.FirstEventDate.AddDays(1));
      Assert.AreEqual(string.Empty, actualResult.ProjectUid, "ProjectUid does not match");
      Assert.AreEqual(3029, actualResult.Code, " result code of request doesn't match expected");
    }


    [TestMethod]
    public void AssetDevice_ProjectMonitoringSubAndProject()
    {
      msg.Title("Project WebTest 4", "Project monitoring subscription and project. Get the projectUid for projectUid request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectV2WebTest1 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
        $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectV2WebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | ProjectV2WebTest4           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectUid(ts, 6, deviceUid.ToString(), 38.837, -121.348, ts.FirstEventDate.AddDays(1));
      Assert.AreEqual(projectUid.ToString(), actualResult.ProjectUid, "ProjectUid does not match");
      Assert.AreEqual(0, actualResult.Code, " result code of request doesn't match expected");
    }

    [TestMethod]
    public void AssetDevice_LandfillSubAndProject()
    {
      msg.Title("Project WebTest 5", "Landfill subscription and project. Get the projectUid for projectUid request");
      var ts = new TestSupport { IsPublishToKafka = false };
      var legacyProjectId = ts.SetLegacyProjectId();
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectV2WebTest5 | 1                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectV2WebTest5 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | CDMA         | ProjectV2WebTest5           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      };
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectUid(ts, 6, deviceUid.ToString(), 38.837, -121.348, ts.FirstEventDate.AddDays(1));
      Assert.AreEqual(projectUid.ToString(), actualResult.ProjectUid, "ProjectUid does not match");
      Assert.AreEqual(0, actualResult.Code, " result code of request doesn't match expected");
    }

    /// <summary>
    /// Call the get project request
    /// </summary>
    /// <param name="ts">test support</param>
    /// <param name="deviceType"></param>
    /// <param name="radioSerial"></param>
    /// <param name="latitude">seed position latitude value from tagfile</param>
    /// <param name="longitude">seed position longitude value from tagfile</param>
    /// <param name="timeOfPosition">from tagfile-used to check against valid Project time range.</param>
    /// <param name="statusCode"></param>
    /// <returns>The project Uid result</returns>
    private GetProjectUidResult CallWebApiGetProjectUid(TestSupport ts, int deviceType, string radioSerial, double latitude,double longitude, DateTime timeOfPosition, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      Thread.Sleep(500);
      var request = GetProjectUidRequest.CreateGetProjectUidRequest(deviceType, radioSerial,latitude,longitude, timeOfPosition);
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v2/project/getUid";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson, statusCode);
      var actualResult = JsonConvert.DeserializeObject<GetProjectUidResult>(response, ts.jsonSettings);
      return actualResult;
    }
  }
}

      
  
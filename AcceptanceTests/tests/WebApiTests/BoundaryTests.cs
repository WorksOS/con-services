using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests
{
  [TestClass]
  public class BoundaryTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    public void GetOneBoundaryForAssetProjectMonitoringManualThree_D_Sub()
    {
      msg.Title("Multiple Boundaries test 1", "Get one boundary for an asset - Project Monitoring - Manual 3D subscription");
      var ts = new TestSupport {IsPublishToKafka = false};
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
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name    | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | MbTest1 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 15               | {startDate} | {endDate}      |               |          |                    |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
    //  $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"
      };
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name    | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | MbTest1 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |" };
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | MbTest1                   |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);
      var actualResult = CallWebApiGetProjectBoundariesAtDateResult(ts, legacyAssetId, ts.FirstEventDate.AddDays(1));
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundaries[0].Boundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }      
      Assert.AreEqual(legacyProjectId, actualResult.projectBoundaries[0].ProjectID, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.result, " result of request doesn't match expected");
    }
  }
}

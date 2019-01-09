using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using TestUtility;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace WebApiTests
{
  [TestClass]
  public class ProjectBoundariesWebTests
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
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | {deviceUid} | CDMA         | MbTest1                   |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);
      var actualResult = CallWebApiGetProjectBoundariesAtDateResult(ts, legacyAssetId, ts.FirstEventDate.AddDays(1));
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundaries[0].Boundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }      
      Assert.AreEqual(legacyProjectId, actualResult.projectBoundaries[0].ProjectID, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void GetOneBoundaryForAssetLandfillManualThree_D_Sub()
    {
      msg.Title("Multiple Boundaries test 2", "Get one boundary for an asset - Landfill project - Manual 3D subscription");
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
      var geometryWKT =
        "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name    | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | MbTest2 | 1                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
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
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | MbTest2 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |" };
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | {deviceUid} | CDMA         | MbTest2                   |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);
      var actualResult = CallWebApiGetProjectBoundariesAtDateResult(ts, legacyAssetId, ts.FirstEventDate.AddDays(1));
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundaries[0].Boundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }      
      Assert.AreEqual(legacyProjectId, actualResult.projectBoundaries[0].ProjectID, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void GetOneBoundaryForAssetLandfillProjectManualThree_D_Sub()
    {
      msg.Title("Multiple Boundaries test 3", "Get one boundary for an asset - Landfill project - Manual 3D subscription");
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
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | MbTest3 | 1                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName            | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer             | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg       | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription         | 0d+09:10:00 | {customerUid} |           |                   | {subscriptionUid} | {customerUid}  | 15               | {startDate} | {endDate}      |               |          |                    |",
      $"| CustomerProject      | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
    //  $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"
      };
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name    | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | MbTest3 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |" };
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | {deviceUid} | CDMA         | MbTest3                   |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |"};
      ts.PublishEventCollection(deviceEventArray);
      var actualResult = CallWebApiGetProjectBoundariesAtDateResult(ts, legacyAssetId, ts.FirstEventDate.AddDays(1));
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      if (actualResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        Assert.IsTrue(actualResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully," Web Api end point not successfull");
      }
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundaries[0].Boundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }      
      Assert.AreEqual(legacyProjectId, actualResult.projectBoundaries[0].ProjectID, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void GetOneBoundaryForAssetProjectMonitoringThree_D_PMSub()
    {
      msg.Title("Multiple Boundaries test 4", "Get one boundary for an asset - Project Monitoring - 3D Project Monitoring subscription");
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
      var geometryWKT =
        "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      // Write events to database 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name    | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | MbTest4 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |                    |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
      };
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name    | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | MbTest4 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |" };
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | {deviceUid} | CDMA         | MbTest4                   |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);
      var actualResult = CallWebApiGetProjectBoundariesAtDateResult(ts, legacyAssetId, ts.FirstEventDate.AddDays(1));
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      if (actualResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        Assert.IsTrue(actualResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully," Web Api end point not successfull");
      }
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundaries[0].Boundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }      
      Assert.AreEqual(legacyProjectId, actualResult.projectBoundaries[0].ProjectID, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void GetOneBoundaryForAssetLandfillThree_D_PMSub()
    {
      msg.Title("Multiple Boundaries test 5", "Get one boundary for an asset - Landfill project - 3D Project Monitoring subscription");
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
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name    | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | MbTest5 | 1                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |                    |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
      };
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name    | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | MbTest5 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |" };
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | {deviceUid} | CDMA         | MbTest5                   |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);
      var actualResult = CallWebApiGetProjectBoundariesAtDateResult(ts, legacyAssetId, ts.FirstEventDate.AddDays(1));
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      if (actualResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        Assert.IsTrue(actualResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully," Web Api end point not successfull");
      }
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundaries[0].Boundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }      
      Assert.AreEqual(legacyProjectId, actualResult.projectBoundaries[0].ProjectID, " Legacy project id's do not match");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void GetTwoBoundariesForAssetLandfillProjectThree_D_PMSubFor2Projects()
    {
      msg.Title("Multiple Boundaries test 6", "Get two boundaries for an asset - Standard and Landfill projects - 3D Project Monitoring subscription");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyProjectId1 = ts.SetLegacyProjectId();
      var legacyProjectId2 = legacyProjectId1+5;
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid1 = Guid.NewGuid();
      var projectUid2 = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT1 = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var geometryWKT2 = "POLYGON((7.62230090191951 49.2081946150602, 7.62204340985408 49.2069679756903, 7.62613109639278 49.2064562828441, 7.63009003689876 49.2060216902407, 7.63120583584896 49.2060076710609, 7.63232163479915 49.2060987956588," + 
                        " 7.63714961102596 49.2067997484898, 7.6390486150085 49.2074376069303, 7.64277152112117 49.2096244876836, 7.64474562695614 49.2114608320588, 7.65029243519893 49.2145235905691, 7.65124730160824 49.2148880251728, 7.6526635079681 49.2149511001198," + 
                        " 7.65453032544246 49.2146357245796, 7.65656880429378 49.2134933474451, 7.65817812970272 49.2131359111401, 7.65985182812801 49.2129887307338, 7.6615255265533 49.2125331695565, 7.66315630963436 49.2108720878569, 7.66421846440426 49.2101922197566," + 
                        " 7.66676119855037 49.2095824331981, 7.66991547635189 49.2091759046471, 7.67163209012142 49.2093791693405, 7.6731233983337 49.2099398951966, 7.6755910306274 49.2102623096841, 7.68140605977169 49.2095543968538, 7.68469981244198 49.2093020690383," + 
                        " 7.68781117489925 49.209519351401, 7.69170574238887 49.2097786871647, 7.69305757573238 49.2109772212652, 7.69301466038814 49.211790245401, 7.6821463494598 49.2112645847722, 7.67506531766048 49.2117692190833, 7.66683630040279 49.2112295405319," + 
                        " 7.66560248425594 49.2121687175933, 7.66217998555294 49.2142432544327, 7.65777043393245 49.214642732949, 7.6546805291473 49.2160724190803, 7.65085033467403 49.216226598448, 7.6446490674316 49.2139699251672, 7.64105490735164 49.2113697173464," + 
                        " 7.63747147610775 49.2084119022941, 7.62230090191951 49.2081946150602))";
 
      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID    | LegacyProjectID    | Name    | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | MbTest6 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT1} |",
      $"| Project   | 4d+09:00:00 | {projectUid2} | {legacyProjectId2} | MbTest6 | 1                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT2} |", 

      };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |                    |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid1} |          |                    |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid2}  |          |                    |",
      };
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name    | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | MbTest6 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |" };
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | {deviceUid} | CDMA         | MbTest6                   |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);
      var actualResult = CallWebApiGetProjectBoundariesAtDateResult(ts, legacyAssetId, ts.FirstEventDate.AddDays(10));
      if (actualResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        Assert.IsTrue(actualResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully," Web Api end point not successfull");
      }

      var expectedgeometryWkt1 = ConvertPolygonToFencePoints(geometryWKT1);
      var expectedgeometryWkt2 = ConvertPolygonToFencePoints(geometryWKT2);
      Assert.IsTrue(actualResult.projectBoundaries.Length == 2, " Expecting 2 boundaries");
      foreach (var boundary in actualResult.projectBoundaries)
      {
        if (boundary.ProjectID == legacyProjectId1)
        {
          for (var resultCnt = 0; resultCnt < expectedgeometryWkt1.FencePoints.Length; resultCnt++)
          {
            Assert.AreEqual(expectedgeometryWkt1.FencePoints[resultCnt], boundary.Boundary.FencePoints[resultCnt]," A fence point on the project boundary does not match");
          }
          Assert.AreEqual(legacyProjectId1, boundary.ProjectID, " Legacy project id's do not match");
        }
        if (boundary.ProjectID == legacyProjectId2)
        {
          for (var resultCnt = 0; resultCnt < expectedgeometryWkt2.FencePoints.Length; resultCnt++)
          {
            Assert.AreEqual(expectedgeometryWkt2.FencePoints[resultCnt], boundary.Boundary.FencePoints[resultCnt]," A fence point on the project boundary does not match");
          }
          Assert.AreEqual(legacyProjectId2, boundary.ProjectID, " Legacy project id's do not match");
        }
      }
    }

    [TestMethod]
    public void GetThreeBoundariesForAssetThree_D_PMSubFor3Projects()
    {
      msg.Title("Multiple Boundaries test 7", "Get three boundaries for an asset - all 3 project - 3D Project Monitoring subscription");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyProjectId1 = ts.SetLegacyProjectId();
      var legacyProjectId2 = legacyProjectId1+5;
      var legacyProjectId3 = legacyProjectId2+5;
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid1 = Guid.NewGuid();
      var projectUid2 = Guid.NewGuid();
      var projectUid3 = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDate = ts.FirstEventDate.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var geometryWKT1 = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var geometryWKT2 = "POLYGON((7.62230090191951 49.2081946150602, 7.62204340985408 49.2069679756903, 7.62613109639278 49.2064562828441, 7.63009003689876 49.2060216902407, 7.63120583584896 49.2060076710609, 7.63232163479915 49.2060987956588," + 
                        " 7.63714961102596 49.2067997484898, 7.6390486150085 49.2074376069303, 7.64277152112117 49.2096244876836, 7.64474562695614 49.2114608320588, 7.65029243519893 49.2145235905691, 7.65124730160824 49.2148880251728, 7.6526635079681 49.2149511001198," + 
                        " 7.65453032544246 49.2146357245796, 7.65656880429378 49.2134933474451, 7.65817812970272 49.2131359111401, 7.65985182812801 49.2129887307338, 7.6615255265533 49.2125331695565, 7.66315630963436 49.2108720878569, 7.66421846440426 49.2101922197566," + 
                        " 7.66676119855037 49.2095824331981, 7.66991547635189 49.2091759046471, 7.67163209012142 49.2093791693405, 7.6731233983337 49.2099398951966, 7.6755910306274 49.2102623096841, 7.68140605977169 49.2095543968538, 7.68469981244198 49.2093020690383," + 
                        " 7.68781117489925 49.209519351401, 7.69170574238887 49.2097786871647, 7.69305757573238 49.2109772212652, 7.69301466038814 49.211790245401, 7.6821463494598 49.2112645847722, 7.67506531766048 49.2117692190833, 7.66683630040279 49.2112295405319," + 
                        " 7.66560248425594 49.2121687175933, 7.66217998555294 49.2142432544327, 7.65777043393245 49.214642732949, 7.6546805291473 49.2160724190803, 7.65085033467403 49.216226598448, 7.6446490674316 49.2139699251672, 7.64105490735164 49.2113697173464," + 
                        " 7.63747147610775 49.2084119022941, 7.62230090191951 49.2081946150602))";
 
      var geometryWKT3 = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.025723657623 36.2101347890754))";

      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID    | LegacyProjectID    | Name    | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT    |",
      $"| Project   | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | MbTest7 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT1} |",
      $"| Project   | 4d+09:00:00 | {projectUid2} | {legacyProjectId2} | MbTest7 | 1                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT2} |", 
      $"| Project   | 4d+09:00:00 | {projectUid3} | {legacyProjectId3} | MbTest7 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT3} |", 
      };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |                    |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid1} |          |                    |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid2} |          |                    |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid3} |          |                    |"
      };
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name    | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | MbTest7 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |" };
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType              | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | {DeviceTypeEnum.SNM940} | {deviceUid} | CDMA         | MbTest7                   |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |                         |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);
      var actualResult = CallWebApiGetProjectBoundariesAtDateResult(ts, legacyAssetId, ts.FirstEventDate.AddDays(10));
      if (actualResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        Assert.IsTrue(actualResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully," Web Api end point not successfull");
      }

      var expectedgeometryWkt1 = ConvertPolygonToFencePoints(geometryWKT1);
      var expectedgeometryWkt2 = ConvertPolygonToFencePoints(geometryWKT2);
      var expectedgeometryWkt3 = ConvertPolygonToFencePoints(geometryWKT3);
      Assert.IsTrue(actualResult.projectBoundaries.Length == 3, " Expecting 3 boundaries");
      foreach (var boundary in actualResult.projectBoundaries)
      {
        if (boundary.ProjectID == legacyProjectId1)
        {
          for (var resultCnt = 0; resultCnt < expectedgeometryWkt1.FencePoints.Length; resultCnt++)
          {
            Assert.AreEqual(expectedgeometryWkt1.FencePoints[resultCnt], boundary.Boundary.FencePoints[resultCnt]," A fence point on the project boundary does not match");
          }
          Assert.AreEqual(legacyProjectId1, boundary.ProjectID, " Legacy project id's do not match");
        }
        if (boundary.ProjectID == legacyProjectId2)
        {
          for (var resultCnt = 0; resultCnt < expectedgeometryWkt2.FencePoints.Length; resultCnt++)
          {
            Assert.AreEqual(expectedgeometryWkt2.FencePoints[resultCnt], boundary.Boundary.FencePoints[resultCnt]," A fence point on the project boundary does not match");
          }
          Assert.AreEqual(legacyProjectId2, boundary.ProjectID, " Legacy project id's do not match");
        }
        if (boundary.ProjectID == legacyProjectId3)
        {
          for (var resultCnt = 0; resultCnt < expectedgeometryWkt3.FencePoints.Length; resultCnt++)
          {
            Assert.AreEqual(expectedgeometryWkt3.FencePoints[resultCnt], boundary.Boundary.FencePoints[resultCnt]," A fence point on the project boundary does not match");
          }
          Assert.AreEqual(legacyProjectId3, boundary.ProjectID, " Legacy project id's do not match");
        }
      }
    }

    /// <summary>
    /// Convert the geometryWkt into the TWGS84FenceContainer returned from the web api
    /// </summary>
    /// <param name="geometryWkt">string set in the test</param>
    /// <returns>TWGS84FenceContainer boundary</returns>
    private TWGS84FenceContainer ConvertPolygonToFencePoints(string geometryWkt)
    {
      try
      {
        var polygon = geometryWkt.Substring(geometryWkt.LastIndexOf('(')+1);
        polygon = polygon.Trim(')');
        var latLongArray = polygon.Split(',');
        var fenceCnt = 0;
        var fenceContainer = new TWGS84FenceContainer
        {
          FencePoints = new TWGS84Point[latLongArray.Length] 
        };     
        foreach (var fencePoints in latLongArray)
        {
          var fence = fencePoints.Trim().Split(' ');
          fenceContainer.FencePoints[fenceCnt] = new TWGS84Point(Convert.ToDouble(fence[0].Trim()), Convert.ToDouble(fence[1].Trim()));
          fenceCnt++;
        }
        return fenceContainer;
      }
      catch (Exception ex)
      {
        msg.DisplayException(ex.Message);        
        throw;
      }
    }


    /// <summary>
    /// Gets a list of project boundaries for the owner of the specified asset which are active at the specified date time. 
    /// </summary>
    /// <param name="ts">testsupport</param>
    /// <param name="assetId">Assett id that the tag file is for</param>
    /// <param name="tagFileUtc">The date/time of the tag file</param>
    /// <returns>A list of  project boundaries, each boundary is a list of WGS84 lat/lng points in radians.</returns>
    private GetProjectBoundariesAtDateResult CallWebApiGetProjectBoundariesAtDateResult(TestSupport ts,long assetId, DateTime tagFileUtc )
    {
      Thread.Sleep(500);
      var request = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(assetId, tagFileUtc);
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/project/getBoundaries";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetProjectBoundariesAtDateResult>(response, ts.jsonSettings);
      return actualResult;
    }
  }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using TestUtility;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests
{
  [TestClass]
  public class ProjectBoundaryWebTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    public void ValidProjectBoundaryForProjectIdAndTagfileDateSixPoints()
    {
      msg.Title("Project Boundary WebTest 1", "Valid project boundary for projectId and tagfile date 6 points. Project Monitoring subscription and project");
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

      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest1 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |EffectiveDate | ",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |               |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |               |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |               |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |               |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest1 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest4           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectBoundaryAtDateResult(ts,legacyProjectId, ts.FirstEventDate);
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }     
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void ValidProjectBoundaryForProjectIdAndTagfileDateFourtyThreePoints()
    {
      msg.Title("Project Boundary WebTest 2", "Valid project boundary for projectId and tagfile date for 43 points. Project Monitoring subscription and project");
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
      var geometryWKT = "POLYGON((7.62230090191951 49.2081946150602, 7.62204340985408 49.2069679756903, 7.62613109639278 49.2064562828441, 7.63009003689876 49.2060216902407, 7.63120583584896 49.2060076710609, 7.63232163479915 49.2060987956588," + 
                        " 7.63714961102596 49.2067997484898, 7.6390486150085 49.2074376069303, 7.64277152112117 49.2096244876836, 7.64474562695614 49.2114608320588, 7.65029243519893 49.2145235905691, 7.65124730160824 49.2148880251728, 7.6526635079681 49.2149511001198," + 
                        " 7.65453032544246 49.2146357245796, 7.65656880429378 49.2134933474451, 7.65817812970272 49.2131359111401, 7.65985182812801 49.2129887307338, 7.6615255265533 49.2125331695565, 7.66315630963436 49.2108720878569, 7.66421846440426 49.2101922197566," + 
                        " 7.66676119855037 49.2095824331981, 7.66991547635189 49.2091759046471, 7.67163209012142 49.2093791693405, 7.6731233983337 49.2099398951966, 7.6755910306274 49.2102623096841, 7.68140605977169 49.2095543968538, 7.68469981244198 49.2093020690383," + 
                        " 7.68781117489925 49.209519351401, 7.69170574238887 49.2097786871647, 7.69305757573238 49.2109772212652, 7.69301466038814 49.211790245401, 7.6821463494598 49.2112645847722, 7.67506531766048 49.2117692190833, 7.66683630040279 49.2112295405319," + 
                        " 7.66560248425594 49.2121687175933, 7.66217998555294 49.2142432544327, 7.65777043393245 49.214642732949, 7.6546805291473 49.2160724190803, 7.65085033467403 49.216226598448, 7.6446490674316 49.2139699251672, 7.64105490735164 49.2113697173464," + 
                        " 7.63747147610775 49.2084119022941, 7.62230090191951 49.2081946150602))";

      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest2 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID | EffectiveDate |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |               |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |               |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |               |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |               |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest2 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest2           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectBoundaryAtDateResult(ts,legacyProjectId, ts.FirstEventDate);
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }      
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void ThreeBoundariesForAssetStandardProjectThree_D_PMSubFor3Projects()
    {
      msg.Title("Project Boundary WebTest 3", "Get one boundary for an asset with three projects. 3D Project Monitoring subscription and 3 different project");
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
       "| TableName | EventDate   | ProjectUID    | LegacyProjectID    | Name     | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT    |",
      $"| Project   | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | wbtest31 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT1} |",
      $"| Project   | 4d+09:00:00 | {projectUid2} | {legacyProjectId2} | wbtest32 | 1                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT2} |", 
      $"| Project   | 4d+09:00:00 | {projectUid3} | {legacyProjectId3} | wbtest33 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT3} |", 
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
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | wbtest3 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |" };
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | wbtest3                   |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);
      var actualResult = CallWebApiGetProjectBoundaryAtDateResult(ts,legacyProjectId2, ts.FirstEventDate);
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT2);
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }      
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void ValidProjectBoundaryForProjectIdLandfillProjectAndSubscription()
    {
      msg.Title("Project Boundary WebTest 4", "Valid project boundary for projectId Landfill Project And Subscription");
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

      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest4 | 1                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID | EffectiveDate |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |               |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |               |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |               |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |               |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest4 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest4           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectBoundaryAtDateResult(ts,legacyProjectId, ts.FirstEventDate);
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }     
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void ValidProjectBoundaryForProjectIdStandardProjectAndThree_D_PM_Subscription()
    {
      msg.Title("Project Boundary WebTest 5", "Valid project boundary for projectId standard project and 3D Project Monitoring subscription");
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

      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest5 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID | EffectiveDate |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |               |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |               |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 13               | {startDate} | {endDate}      |               |          |                    |               |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |               |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest5 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate |",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest5           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectBoundaryAtDateResult(ts,legacyProjectId, ts.FirstEventDate);
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }     
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void ValidProjectBoundaryForProjectIdStandardProjectAndManualThree_D_PM_Subscription()
    {
      msg.Title("Project Boundary WebTest 6", "Valid project boundary for projectId standard project and Manual 3D Project Monitoring subscription");
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

      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest6 | 0                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID | EffectiveDate |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |               |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |               |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 15               | {startDate} | {endDate}      |               |          |                    |               |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |               |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest6 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate |",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest6           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectBoundaryAtDateResult(ts,legacyProjectId, ts.FirstEventDate);
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }     
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }


    [TestMethod]
    public void ValidProjectBoundaryForProjectIdProjectMonitoringProjectAndManualThree_D_PM_Subscription()
    {
      msg.Title("Project Boundary WebTest 7", "Valid project boundary for projectId project monitoring project and Manual 3D Project Monitoring subscription");
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

      var projectEventArray = new[] {
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name            | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:00:00 | {projectUid} | {legacyProjectId} | ProjectWebTest7 | 2                | New Zealand Standard Time | Pacific/Auckland | {startDate} | {endDate} | {geometryWKT} |" };
      ts.PublishEventCollection(projectEventArray);
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID | EffectiveDate |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |               |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |               |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 15               | {startDate} | {endDate}      |               |          |                    |               |",
      $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |               |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(eventsArray);
      var assetEventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name            | MakeCode | SerialNumber | Model | IconKey | AssetType  | OwningCustomerUID |",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | ProjectWebTest7 | CAT      | XAT1         | 345D  | 10      | Excavators | {customerUid}     |"};
      ts.PublishEventCollection(assetEventArray);
      var deviceEventArray = new[] {
       "| TableName         | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | GatewayFirmwarePartNumber | fk_AssetUID   | fk_DeviceUID | fk_SubscriptionUID | EffectiveDate | ",
      $"| Device            | 0d+09:00:00 | {deviceUid}        | Subscribed  | Series522  | {deviceUid} | CDMA         | ProjectWebTest7           |               |              |                    |               |",
      $"| AssetDevice       | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} | {deviceUid}  |                    |               |",
      $"| AssetSubscription | 0d+09:20:00 |                    |             |            |             |              |                           | {ts.AssetUid} |              | {subscriptionUid}  | {startDate}   |"};
      ts.PublishEventCollection(deviceEventArray);

      var actualResult = CallWebApiGetProjectBoundaryAtDateResult(ts,legacyProjectId, ts.FirstEventDate);
      var expectedResult = ConvertPolygonToFencePoints(geometryWKT);
      for (var resultCnt = 0; resultCnt < expectedResult.FencePoints.Length; resultCnt++)
      {
        Assert.AreEqual(expectedResult.FencePoints[resultCnt], actualResult.projectBoundary.FencePoints[resultCnt], " A fence point on the project boundary does not match");
      }     
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
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
    /// Call the project/getBoundary endpoint. Gets the project boundary for the specified project if it is active at the specified date time. 
    /// </summary>
    /// <param name="ts">testsupport</param>
    /// <param name="projectId">Project id that the tag file is for</param>
    /// <param name="tagFileUtc">The date/time of the tag file</param>
    /// <returns>The project boundary as a list of WGS84 lat/lng points in radians.</returns>
    private GetProjectBoundaryAtDateResult CallWebApiGetProjectBoundaryAtDateResult(TestSupport ts,long projectId, DateTime tagFileUtc )
    {  
      Thread.Sleep(500);          
      var request = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(projectId, tagFileUtc);
      var requestJson = JsonConvert.SerializeObject(request, ts.jsonSettings);
      var restClient = new RestClient();
      var uri = ts.GetBaseUri() + "api/v1/project/getBoundary";
      var method = HttpMethod.Post.ToString();
      var response = restClient.DoHttpRequest(uri, method, requestJson);
      var actualResult = JsonConvert.DeserializeObject<GetProjectBoundaryAtDateResult>(response, ts.jsonSettings);
      return actualResult;
    }
  }
}

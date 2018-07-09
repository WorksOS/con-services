using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using TestUtility;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace IntegrationTests
{
  [TestClass]
  public class ProjectIntTests
  {
    private readonly Msg msg = new Msg();
    [TestMethod]
    public void ThreeDPMSubscription_GetProjectId()
    {
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
       "| EventType          | EventDate   | ProjectID         | ProjectUID    | ProjectName     | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |",
      $"| CreateProjectEvent | 1d+09:00:00 | {legacyProjectId} | {projectUid}  | ProjectIntTest1 | {ProjectType.Standard} | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWkt} |"};
      ts.PublishEventCollection(projectEventArray);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid);
      var endDt = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
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
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void ManualPMSubscription_GetProjectId()
    {
      // Manual3d customer service type is not acceptable for a project type of Standard
      msg.Title("Project Int Test 2", "Inject Asset,Device,Project and customer events with Manual 3D Project Monitoring subscription. Call projects/getId and return project id");
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
       "| EventType          | EventDate   | ProjectID         | ProjectUID    | ProjectName     | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |",
      $"| CreateProjectEvent | 1d+09:00:00 | {legacyProjectId} | {projectUid}  | ProjectIntTest2 | {ProjectType.Standard} | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWkt} |"};
      ts.PublishEventCollection(projectEventArray);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid);
      var endDt = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var custEventArray = new[] {
       "| EventType                         | EventDate   | CustomerName | CustomerType | CustomerUID   | SubscriptionUID   | StartDate          | EndDate | SubscriptionType             | ",
      $"| CreateCustomerEvent               | 0d+09:00:00 | CustName     | Customer     | {customerUid} |                   |                    |         |                              | ",
      $"| CreateCustomerSubscriptionEvent   | 0d+09:01:00 |              |              | {customerUid} | {subscriptionUid} |{ts.FirstEventDate} | {endDt} | Manual 3D Project Monitoring | "};
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
      Assert.AreEqual(-1, actualResult.projectId, " Legacy project id's do not match");     
    }

    [TestMethod]
    public void LandfillSubscription_GetProjectId()
    {
      msg.Title("Project Int Test 3", "Inject Asset,Device,Project and customer events with Landfill subscription. Call projects/getId and return project id");
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
       "| EventType          | EventDate   | ProjectID         | ProjectUID    | ProjectName     | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |",
      $"| CreateProjectEvent | 1d+09:00:00 | {legacyProjectId} | {projectUid}  | ProjectIntTest3 | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWkt} |"};
      ts.PublishEventCollection(projectEventArray);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid);
      var endDt = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var custEventArray = new[] {
       "| EventType                         | EventDate   | CustomerName | CustomerType | CustomerUID   | ProjectUID   | SubscriptionUID   | StartDate          | EndDate | SubscriptionType | DeviceUID   | AssetUID      |",
      $"| CreateCustomerEvent               | 0d+09:00:00 | CustName     | Customer     | {customerUid} |              |                   |                    |         |                  |             |               |",
      $"| CreateProjectSubscriptionEvent    | 0d+09:01:00 |              |              | {customerUid} |              | {subscriptionUid} |{ts.FirstEventDate} | {endDt} | Landfill         | {deviceUid} | {ts.AssetUid} |"};
      ts.PublishEventCollection(custEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);

      var associateEventArray = new[] {
       "| EventType                         | EventDate   | ProjectUID    | CustomerUID   | SubscriptionUID   | ",
      $"| AssociateProjectCustomer          | 0d+09:00:00 | {projectUid}  | {customerUid} |                   | ",
      $"| AssociateProjectSubscriptionEvent | 0d+09:00:00 | {projectUid}  |               | {subscriptionUid} |"};
      ts.PublishEventCollection(associateEventArray);
      ts.IsPublishToKafka = false;
      var custTccOrg = new[] {
        "| TableName       | EventDate   | CustomerUID   | TCCOrgID |",
       $"| CustomerTccOrg  | 0d+09:00:00 | {customerUid} | {tccOrg} |"};
      ts.PublishEventCollection(custTccOrg);

      var actualResult = CallWebApiGetProjectId(ts, legacyAssetId, 38.837, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(legacyProjectId, actualResult.projectId, " Legacy asset id's do not match");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }


   [TestMethod]
    public void ProjectMonitoSubscription_GetProjectId()
    {
      msg.Title("Project Int Test 4", "Inject Asset,Device,Project and customer events with Landfill subscription. Call projects/getId and return project id");
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
       "| EventType          | EventDate   | ProjectID         | ProjectUID    | ProjectName     | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |",
      $"| CreateProjectEvent | 1d+09:00:00 | {legacyProjectId} | {projectUid}  | ProjectIntTest4 | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWkt} |"};
      ts.PublishEventCollection(projectEventArray);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid);
      var endDt = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var custEventArray = new[] {
       "| EventType                         | EventDate   | CustomerName | CustomerType | CustomerUID   | ProjectUID   | SubscriptionUID   | StartDate          | EndDate | SubscriptionType | DeviceUID   | AssetUID      |",
      $"| CreateCustomerEvent               | 0d+09:00:00 | CustName     | Customer     | {customerUid} |              |                   |                    |         |                  |             |               |",
      $"| CreateProjectSubscriptionEvent      | 0d+09:01:00 |              |              | {customerUid} |              | {subscriptionUid} |{ts.FirstEventDate} | {endDt} | Landfill         | {deviceUid} | {ts.AssetUid} |"};
      ts.PublishEventCollection(custEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);

      var associateEventArray = new[] {
       "| EventType                         | EventDate   | ProjectUID    | CustomerUID   | SubscriptionUID   | ",
      $"| AssociateProjectCustomer          | 0d+09:00:00 | {projectUid}  | {customerUid} |                   | ",
      $"| AssociateProjectSubscriptionEvent | 0d+09:00:00 | {projectUid}  |               | {subscriptionUid} |"};
      ts.PublishEventCollection(associateEventArray);
      ts.IsPublishToKafka = false;
      var custTccOrg = new[] {
        "| TableName       | EventDate   | CustomerUID   | TCCOrgID |",
       $"| CustomerTccOrg  | 0d+09:00:00 | {customerUid} | {tccOrg} |"};
      ts.PublishEventCollection(custTccOrg);

      var actualResult = CallWebApiGetProjectId(ts, legacyAssetId, 38.837, -121.348, ts.FirstEventDate.AddDays(1), tccOrg.ToString());
      Assert.AreEqual(legacyProjectId, actualResult.projectId, " Legacy asset id's do not match");
      Assert.AreEqual(true, actualResult.Result, " result of request doesn't match expected");
    }

    [TestMethod]
    public void ThreeDPMSubscription_GetBoundaries()
    {
      msg.Title("Project boundaries test 1", "Inject Asset,Device,Project and customer events with 3D subscription. Call projects/getBoundaries");
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var legacyProjectId1 = ts.SetLegacyProjectId();
      var legacyProjectId2 = legacyProjectId1+5;
      var legacyProjectId3 = legacyProjectId2+5;
      var legacyAssetId = ts.SetLegacyAssetId();
      var projectUid1 = Guid.NewGuid();
      var projectUid2 = Guid.NewGuid();
      var projectUid3 = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var deviceUid = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
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

      var deviceEventArray = new[] {
       "| EventType                 | EventDate   | DeviceSerialNumber | DeviceState | DeviceType | DeviceUID   | DataLinkType | AssetUID      | AssetName   | Make | SerialNumber | Model | IconKey | AssetType  | LegacyAssetId   | OwningCustomerUID |",
      $"| CreateDeviceEvent         | 0d+09:00:00 | {deviceUid}        | Subscribed  | SNM940     | {deviceUid} | 4G           |               |             |      |              |       |         |            |                 |                   |",
      $"| CreateAssetEvent          | 0d+09:05:00 |                    |             |            |             |              | {ts.AssetUid} | ProjectBnd1 | CAT  | XAT1         | 374D  | 10      | Excavators | {legacyAssetId} | {customerUid}     |",
      $"| AssociateDeviceAssetEvent | 0d+09:10:00 |                    |             |            | {deviceUid} |              | {ts.AssetUid} |             |      |              |       |         |            |                 |                   |"};
      ts.PublishEventCollection(deviceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Device", "DeviceUID", 1, deviceUid);
      mysql.VerifyTestResultDatabaseRecordCount("Asset", "AssetUID", 1, new Guid(ts.AssetUid));

      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectID          | ProjectUID     | ProjectName     | ProjectType                     | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT    |",
      $"| CreateProjectEvent | 1d+09:00:00 | {legacyProjectId1} | {projectUid1}  | ProjectBndTest1 | {ProjectType.Standard}          | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWKT1} |",
      $"| CreateProjectEvent | 1d+09:00:00 | {legacyProjectId2} | {projectUid2}  | ProjectBndTest1 | {ProjectType.LandFill}          | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWKT2} |",
      $"| CreateProjectEvent | 1d+09:00:00 | {legacyProjectId3} | {projectUid3}  | ProjectBndTest1 | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      | {geometryWKT3} |"};

      ts.PublishEventCollection(projectEventArray);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid1);
      var endDt = new DateTime(9999, 12, 31).ToString("yyyy-MM-dd");
      var custEventArray = new[] {
       "| EventType                         | EventDate   | CustomerName | CustomerType | CustomerUID   | ProjectUID   | SubscriptionUID   | StartDate          | EndDate | SubscriptionType      | DeviceUID   | AssetUID      |",
      $"| CreateCustomerEvent               | 0d+09:00:00 | CustName     | Customer     | {customerUid} |              |                   |                    |         |                       |             |               |",
      $"| CreateAssetSubscriptionEvent      | 0d+09:01:00 |              |              | {customerUid} |              | {subscriptionUid} |{ts.FirstEventDate} | {endDt} | 3D Project Monitoring | {deviceUid} | {ts.AssetUid} |"};
      ts.PublishEventCollection(custEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);

      var associateEventArray = new[] {
       "| EventType                | EventDate   | ProjectUID    | CustomerUID   | ",
      $"| AssociateProjectCustomer | 0d+09:00:00 | {projectUid1} | {customerUid} | ",
      $"| AssociateProjectCustomer | 0d+09:00:00 | {projectUid2} | {customerUid} | ",
      $"| AssociateProjectCustomer | 0d+09:00:00 | {projectUid3} | {customerUid} | "};
      ts.PublishEventCollection(associateEventArray);

      ts.IsPublishToKafka = false;
      var custTccOrg = new[] {
        "| TableName       | EventDate   | CustomerUID   | TCCOrgID |",
       $"| CustomerTccOrg  | 0d+09:00:00 | {customerUid} | {tccOrg} |"};
      ts.PublishEventCollection(custTccOrg);
      var actualResult = CallWebApiGetProjectBoundariesAtDateResult(ts, legacyAssetId, ts.FirstEventDate.AddDays(10));
      if (actualResult.Code != VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum.ExecutedSuccessfully)
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

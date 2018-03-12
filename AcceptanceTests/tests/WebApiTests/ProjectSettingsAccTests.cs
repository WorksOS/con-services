using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestUtility;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests
{
  [TestClass]
  public class ProjectSettingsAccTests
  {
    private readonly Msg msg = new Msg();
    private const string PROJECT_DB_SCHEMA_NAME = "VSS-MasterData-Project-Only";

    [TestMethod]
    public void AddProjectSettingsGoodPath()
    {
      msg.Title("Project settings 1", "Add project settings for a standard project");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var customerEventArray = new[] {
        "| TableName | EventDate   | Name              | fk_CustomerTypeID | CustomerUID   |",
       $"| Customer  | 0d+09:00:00 | Projectsettings 1 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Projectsettings 1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
      // Now create the settings
      var projectSettings1 = "{ useMachineTargetPassCount: false,customTargetPassCountMinimum: 5,customTargetPassCountMaximum: 7,useMachineTargetTemperature: false,customTargetTemperatureMinimum: 75," +
      "customTargetTemperatureMaximum: 150,useMachineTargetCmv: false,customTargetCmv: 77,useMachineTargetMdp: false,customTargetMdp: 88,useDefaultTargetRangeCmvPercent: false," +
      "customTargetCmvPercentMinimum: 75,customTargetCmvPercentMaximum: 105,useDefaultTargetRangeMdpPercent: false,customTargetMdpPercentMinimum: 85,customTargetMdpPercentMaximum: 115," +
      "useDefaultTargetRangeSpeed: false,customTargetSpeedMinimum: 10,customTargetSpeedMaximum: 30,useDefaultCutFillTolerances: false,customCutFillTolerances: [3, 2, 1, 0, -1, -2, -3]," + 
      "useDefaultVolumeShrinkageBulking: false, customShrinkagePercent: 5, customBulkingPercent: 7.5}";

      projectSettings1 = projectSettings1.Replace(" ", String.Empty);

      var projSettings1 = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings1, ProjectSettingsType.Targets);
      var configJson1 = JsonConvert.SerializeObject(projSettings1, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var putresponse1 = ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson1, customerUid.ToString());
      var putobjresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(putresponse1);

      var tempSettings = JsonConvert.SerializeObject(putobjresp1.settings).Replace("\"", String.Empty);
      
      //Assert.AreEqual(projectSettings1, putobjresp1.settings, "Actual project settings 1 do not match expected");
      Assert.AreEqual(projectSettings1, tempSettings, "Actual project settings 1 do not match expected");
      Assert.AreEqual(projectUid, putobjresp1.projectUid, "Actual project Uid for project settings 1 do not match expected");

      // create settings for a second user for same project
      var projectSettings2 = "{ useMachineTargetPassCount: false,customTargetPassCountMinimum: 6,customTargetPassCountMaximum: 6,useMachineTargetTemperature: false,customTargetTemperatureMinimum: 70," +
                             "customTargetTemperatureMaximum: 140,useMachineTargetCmv: false,customTargetCmv: 71,useMachineTargetMdp: false,customTargetMdp: 81,useDefaultTargetRangeCmvPercent: false," +
                             "customTargetCmvPercentMinimum: 80,customTargetCmvPercentMaximum: 100,useDefaultTargetRangeMdpPercent: false,customTargetMdpPercentMinimum: 80,customTargetMdpPercentMaximum: 100," +
                             "useDefaultTargetRangeSpeed: false,customTargetSpeedMinimum: 12,customTargetSpeedMaximum: 27,useDefaultCutFillTolerances: false,customCutFillTolerances: [3, 2, 1, 0, -1, -2, -3]," +
                             "useDefaultVolumeShrinkageBulking: false, customShrinkagePercent: 6, customBulkingPercent: 5.2}";

      projectSettings2 = projectSettings2.Replace(" ", String.Empty);

      var projSettings2 = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings2, ProjectSettingsType.Targets);
      var configJson2 = JsonConvert.SerializeObject(projSettings2, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var putresponse2 = ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson2, customerUid.ToString(), RestClientUtil.ANOTHER_JWT);
      var putobjresp2 = JsonConvert.DeserializeObject<ProjectSettingsResult>(putresponse2);

      tempSettings = JsonConvert.SerializeObject(putobjresp2.settings).Replace("\"", String.Empty);

      //Assert.AreEqual(projectSettings2, putobjresp2.settings, "Actual project settings 2 do not match expected");
      Assert.AreEqual(projectSettings2, tempSettings, "Actual project settings 2 do not match expected");
      Assert.AreEqual(projectUid, putobjresp2.projectUid, "Actual project Uid for project settings 2 do not match expected");
      
      
      // get call
      var getresponse1 = ts.CallProjectWebApiV4($"api/v4/projectsettings/{projectUid}", "GET", null, customerUid.ToString());
      var getobjresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(getresponse1);

      tempSettings = JsonConvert.SerializeObject(getobjresp1.settings).Replace("\"", String.Empty);

      //Assert.AreEqual(projectSettings1, getobjresp1.settings, "Actual project settings do not match expected");
      Assert.AreEqual(projectSettings1, tempSettings, "Actual project settings do not match expected");
      Assert.AreEqual(projectUid, getobjresp1.projectUid, "Actual project Uid for project settings do not match expected");
    }

    [TestMethod]
    public void AddInvalidProjectSettings()
    {
      msg.Title("Project settings 2", "Add project settings for a standard project with invalid project UID");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var customerEventArray = new[] {
        "| TableName | EventDate   | Name              | fk_CustomerTypeID | CustomerUID   |",
       $"| Customer  | 0d+09:00:00 | Projectsettings 2 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectSettings = "{ Invalid project UID }";
      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings, ProjectSettingsType.Targets);
      var configJson = JsonConvert.SerializeObject(projSettings, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson, customerUid.ToString());
      Assert.IsTrue(response == "{\"Code\":2001,\"Message\":\"No access to the project for a customer or the project does not exist.\"}", "Actual response different to expected") ;
      // Try to get the project that doesn't exist
      var response1 = ts.CallProjectWebApiV4($"api/v4/projectsettings/{projectUid}", "GET", null, customerUid.ToString());
      Assert.IsTrue(response1 == "{\"Code\":2001,\"Message\":\"No access to the project for a customer or the project does not exist.\"}", "Actual response different to expected");
    }

    [TestMethod]
    public void AddProjectSettingsForProjectMonitoringProject()
    {
      msg.Title("Project settings 3", "Add project settings for a project monitoring project");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
        "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
       $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
       $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
       $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      };
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectName       | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
       $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Projectsettings 3 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
      // Now create the settings
      var projectSettings = "{ useMachineTargetPassCount: false,customTargetPassCountMinimum: 5,customTargetPassCountMaximum: 7,useMachineTargetTemperature: false,customTargetTemperatureMinimum: 75," +
                            "customTargetTemperatureMaximum: 150,useMachineTargetCmv: false,customTargetCmv: 77,useMachineTargetMdp: false,customTargetMdp: 88,useDefaultTargetRangeCmvPercent: false," +
                            "customTargetCmvPercentMinimum: 75,customTargetCmvPercentMaximum: 105,useDefaultTargetRangeMdpPercent: false,customTargetMdpPercentMinimum: 85,customTargetMdpPercentMaximum: 115," +
                            "useDefaultTargetRangeSpeed: false,customTargetSpeedMinimum: 10,customTargetSpeedMaximum: 30,useDefaultCutFillTolerances: false,customCutFillTolerances: [3, 2, 1, 0, -1, -2, -3]," +
                            "useDefaultVolumeShrinkageBulking: false, customShrinkagePercent: 5, customBulkingPercent: 7.5}";

      projectSettings = projectSettings.Replace(" ", String.Empty);

      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings, ProjectSettingsType.Targets);
      var configJson = JsonConvert.SerializeObject(projSettings, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<ProjectSettingsResult>(response);

      var tempSettings = JsonConvert.SerializeObject(objresp.settings).Replace("\"", String.Empty);

      Assert.AreEqual(projectSettings, tempSettings, "Actual project settings do not match expected");
      Assert.AreEqual(projectUid, objresp.projectUid, "Actual project Uid for project settings do not match expected");

      // get call
      var response1 = ts.CallProjectWebApiV4($"api/v4/projectsettings/{projectUid}", "GET", null, customerUid.ToString());
      var objresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(response1);

      tempSettings = JsonConvert.SerializeObject(objresp1.settings).Replace("\"", String.Empty);

      Assert.AreEqual(projectSettings, tempSettings, "Actual project settings do not match expected");
      Assert.AreEqual(projectUid, objresp1.projectUid, "Actual project Uid for project settings do not match expected");
    }

    [TestMethod]
    public void AddEmptyProjectSettingsForProjectMonitoringProject()
    {
      msg.Title("Project settings 4", "Add project settings for a project monitoring project");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
        "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
       $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
       $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
       $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      };
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectName       | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
       $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Projectsettings 4 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
      // Now create the settings
      var projectSettings = string.Empty;
      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings, ProjectSettingsType.Targets);
      var configJson = JsonConvert.SerializeObject(projSettings, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<ProjectSettingsResult>(response);

      var tempSettings = objresp.settings == null ? String.Empty : JsonConvert.SerializeObject(objresp.settings).Replace("\"", String.Empty);

      Assert.AreEqual(projectSettings, tempSettings, "Actual project settings do not match expected");
      //Assert.AreEqual(projectUid, objresp.projectUid, "Actual project Uid for project settings do not match expected");

      // get call
      var response1 = ts.CallProjectWebApiV4($"api/v4/projectsettings/{projectUid}", "GET", null, customerUid.ToString());
      var objresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(response1);

      tempSettings = objresp1.settings == null ? String.Empty : JsonConvert.SerializeObject(objresp1.settings).Replace("\"", String.Empty);

      Assert.AreEqual(projectSettings, tempSettings, "Actual project settings do not match expected");
      //Assert.AreEqual(projectUid, objresp1.projectUid, "Actual project Uid for project settings do not match expected");
    }

    [TestMethod]
    public void AddProjectSettingsThenUpdateThem()
    {
      msg.Title("Project settings 5", "Add project settings for a project monitoring project");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
        "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
       $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
       $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
       $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      };
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectName       | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
       $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Projectsettings 3 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
      // Now create the settings
      var projectSettings = "{useMachineTargetPassCount: false,customTargetPassCountMinimum: 5}";

      projectSettings = projectSettings.Replace(" ", String.Empty);

      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings, ProjectSettingsType.Targets);
      var configJson = JsonConvert.SerializeObject(projSettings, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson, customerUid.ToString());

      var projectSettings1 = "{customTargetPassCountMaximum: 7,useMachineTargetTemperature: false,customTargetTemperatureMinimum: 75}";

      projectSettings1 = projectSettings1.Replace(" ", String.Empty);

      var projSettings1 = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings1, ProjectSettingsType.Targets);
      var configJson2 = JsonConvert.SerializeObject(projSettings1, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response1 = ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson2, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<ProjectSettingsResult>(response1);

      var tempSettings = JsonConvert.SerializeObject(objresp.settings).Replace("\"", String.Empty);

      Assert.AreEqual(projectSettings1, tempSettings, "Actual project settings do not match expected");
      Assert.AreEqual(projectUid, objresp.projectUid, "Actual project Uid for project settings do not match expected");

      // get call
      var response2 = ts.CallProjectWebApiV4($"api/v4/projectsettings/{projectUid}", "GET", null, customerUid.ToString());
      var objresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(response2);

      tempSettings = JsonConvert.SerializeObject(objresp1.settings).Replace("\"", String.Empty);

      Assert.AreEqual(projectSettings1, tempSettings, "Actual project settings do not match expected");
      Assert.AreEqual(projectUid, objresp1.projectUid, "Actual project Uid for project settings do not match expected");
    }

    [TestMethod]
    public void AddProjectSettingsAndCheckKafkaMessagesConsumed()
    {
      msg.Title("Project settings 6", "Add project settings then check kafka messages consumed");
      var ts = new TestSupport();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
        "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
       $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
       $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
       $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      };
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectName       | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
       $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Projectsettings 3 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      // Now create the settings
      var projectSettings = "{useMachineTargetPassCount: false,customTargetPassCountMinimum: 5}";

      projectSettings = projectSettings.Replace(" ", String.Empty);

      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings, ProjectSettingsType.Targets);
      var configJson = JsonConvert.SerializeObject(projSettings, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson, customerUid.ToString());

      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("ProjectSettings", "fk_ProjectUID", 1, new Guid(projectUid));
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectSettings", "fk_ProjectUID", "Settings", $"{projectSettings}", new Guid(projectUid));

      var projectSettings1 = "{useMachineTargetPassCount: false,customTargetPassCountMinimum: 5}";

      projectSettings1 = projectSettings1.Replace(" ", String.Empty);

      var projSettings1 = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings1, ProjectSettingsType.Targets);
      var configJson2 = JsonConvert.SerializeObject(projSettings1, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response1 = ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson2, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<ProjectSettingsResult>(response1);

      var tempSettings = JsonConvert.SerializeObject(objresp.settings).Replace("\"", String.Empty);

      Assert.AreEqual(projectSettings1, tempSettings, "Actual project settings do not match expected");
      Assert.AreEqual(projectUid, objresp.projectUid, "Actual project Uid for project settings do not match expected");

      var response2 = ts.CallProjectWebApiV4($"api/v4/projectsettings/{projectUid}", "GET", null, customerUid.ToString());
      var objresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(response2);

      tempSettings = JsonConvert.SerializeObject(objresp1.settings).Replace("\"", String.Empty);

      Assert.AreEqual(projectSettings1, tempSettings, "Actual project settings do not match expected");
      Assert.AreEqual(projectUid, objresp1.projectUid, "Actual project Uid for project settings do not match expected");

      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("ProjectSettings", "fk_ProjectUID", 1, new Guid(projectUid));
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectSettings", "fk_ProjectUID", "Settings", $"{projectSettings1}", new Guid(projectUid));
    }
  }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using Newtonsoft.Json;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

namespace WebApiTests
{
  [TestClass]
  public class ProjectSettingsAccTests
  {
    private readonly Msg msg = new Msg();

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
      var projectSettings = "{ useMachineTargetPassCount: false,customTargetPassCountMinimum: 5,customTargetPassCountMaximum: 7,useMachineTargetTemperature: false,customTargetTemperatureMinimum: 75," +
      "customTargetTemperatureMaximum: 150,useMachineTargetCmv: false,customTargetCmv: 77,useMachineTargetMdp: false,customTargetMdp: 88,useDefaultTargetRangeCmvPercent: false," +
      "customTargetCmvPercentMinimum: 75,customTargetCmvPercentMaximum: 105,useDefaultTargetRangeMdpPercent: false,customTargetMdpPercentMinimum: 85,customTargetMdpPercentMaximum: 115," +
      "useDefaultTargetRangeSpeed: false,customTargetSpeedMinimum: 10,customTargetSpeedMaximum: 30,useDefaultCutFillTolerances: false,customCutFillTolerances: [3, 2, 1, 0, -1, -2, -3]," + 
      "useDefaultVolumeShrinkageBulking: false, customShrinkagePercent: 5, customBulkingPercent: 7.5}";
      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings);
      var configJson = JsonConvert.SerializeObject(projSettings, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<ProjectSettingsResult>(response);
      Assert.AreEqual(objresp.Settings, projectSettings, "Actual project settings do not match expected");
      Assert.AreEqual(objresp.ProjectUid, projectUid, "Actual project Uid for project settings do not match expected");
      // get call
      var response1 = ts.CallProjectWebApiV4($"api/v4/projectsettings/{projectUid}", "GET", null, customerUid.ToString());
      var objresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(response1);
      Assert.AreEqual(objresp1.Settings, projectSettings, "Actual project settings do not match expected");
      Assert.AreEqual(objresp1.ProjectUid, projectUid, "Actual project Uid for project settings do not match expected");

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
      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings);
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
      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings);
      var configJson = JsonConvert.SerializeObject(projSettings, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<ProjectSettingsResult>(response);
      Assert.AreEqual(objresp.Settings, projectSettings, "Actual project settings do not match expected");
      Assert.AreEqual(objresp.ProjectUid, projectUid, "Actual project Uid for project settings do not match expected");
      // get call
      var response1 = ts.CallProjectWebApiV4($"api/v4/projectsettings/{projectUid}", "GET", null, customerUid.ToString());
      var objresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(response1);
      Assert.AreEqual(objresp1.Settings, projectSettings, "Actual project settings do not match expected");
      Assert.AreEqual(objresp1.ProjectUid, projectUid, "Actual project Uid for project settings do not match expected");
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
      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings);
      var configJson = JsonConvert.SerializeObject(projSettings, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = ts.CallProjectWebApiV4("api/v4/projectsettings", "PUT", configJson, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<ProjectSettingsResult>(response);
      Assert.AreEqual(objresp.Settings, projectSettings, "Actual project settings do not match expected");
      Assert.AreEqual(objresp.ProjectUid, projectUid, "Actual project Uid for project settings do not match expected");
      // get call
      var response1 = ts.CallProjectWebApiV4($"api/v4/projectsettings/{projectUid}", "GET", null, customerUid.ToString());
      var objresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(response1);
      Assert.AreEqual(objresp1.Settings, projectSettings, "Actual project settings do not match expected");
      Assert.AreEqual(objresp1.ProjectUid, projectUid, "Actual project Uid for project settings do not match expected");
    }
  }
}

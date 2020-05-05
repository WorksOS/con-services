using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IntegrationTests.UtilityClasses;
using Newtonsoft.Json;
using TestUtility;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace IntegrationTests.WebApiTests
{
  public class ProjectSettingsAccTests : WebApiTestsBase
  {
    [Fact]
    public async Task AddProjectSettingsGoodPath()
    {
      const string testText = "Project settings test 1";
      Msg.Title(testText, "Add project settings for a standard project");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);
      
      // Now create the settings
      var projectSettings1 = "{ useMachineTargetPassCount: false,customTargetPassCountMinimum: 5,customTargetPassCountMaximum: 7,useMachineTargetTemperature: false,customTargetTemperatureMinimum: 75," +
      "customTargetTemperatureMaximum: 150,useMachineTargetCmv: false,customTargetCmv: 77,useMachineTargetMdp: false,customTargetMdp: 88,useDefaultTargetRangeCmvPercent: false," +
      "customTargetCmvPercentMinimum: 75,customTargetCmvPercentMaximum: 105,useDefaultTargetRangeMdpPercent: false,customTargetMdpPercentMinimum: 85,customTargetMdpPercentMaximum: 115," +
      "useDefaultTargetRangeSpeed: false,customTargetSpeedMinimum: 10,customTargetSpeedMaximum: 30,useDefaultCutFillTolerances: false,customCutFillTolerances: [3, 2, 1, 0, -1, -2, -3]," +
      "useDefaultVolumeShrinkageBulking: false, customShrinkagePercent: 5, customBulkingPercent: 7.5}";

      projectSettings1 = projectSettings1.Replace(" ", string.Empty);

      var projSettings1 = ProjectSettingsRequest.CreateProjectSettingsRequest(ts.ProjectUid.ToString(), projectSettings1, ProjectSettingsType.Targets);
      var configJson1 = JsonConvert.SerializeObject(projSettings1, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var putresponse1 = await ts.CallProjectWebApi("api/v4/projectsettings", HttpMethod.Put, configJson1, customerUid.ToString());
      var putobjresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(putresponse1);

      var tempSettings = JsonConvert.SerializeObject(putobjresp1.settings).Replace("\"", string.Empty);

      //Assert.Equal(projectSettings1, putobjresp1.settings, "Actual project settings 1 do not match expected");
      Assert.Equal(projectSettings1, tempSettings);
      Assert.Equal(ts.ProjectUid.ToString(), putobjresp1.projectUid);

      // create settings for a second user for same project
      var projectSettings2 = "{ useMachineTargetPassCount: false,customTargetPassCountMinimum: 6,customTargetPassCountMaximum: 6,useMachineTargetTemperature: false,customTargetTemperatureMinimum: 70," +
                             "customTargetTemperatureMaximum: 140,useMachineTargetCmv: false,customTargetCmv: 71,useMachineTargetMdp: false,customTargetMdp: 81,useDefaultTargetRangeCmvPercent: false," +
                             "customTargetCmvPercentMinimum: 80,customTargetCmvPercentMaximum: 100,useDefaultTargetRangeMdpPercent: false,customTargetMdpPercentMinimum: 80,customTargetMdpPercentMaximum: 100," +
                             "useDefaultTargetRangeSpeed: false,customTargetSpeedMinimum: 12,customTargetSpeedMaximum: 27,useDefaultCutFillTolerances: false,customCutFillTolerances: [3, 2, 1, 0, -1, -2, -3]," +
                             "useDefaultVolumeShrinkageBulking: false, customShrinkagePercent: 6, customBulkingPercent: 5.2}";

      projectSettings2 = projectSettings2.Replace(" ", string.Empty);

      var projSettings2 = ProjectSettingsRequest.CreateProjectSettingsRequest(ts.ProjectUid.ToString(), projectSettings2, ProjectSettingsType.Targets);
      var configJson2 = JsonConvert.SerializeObject(projSettings2, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var putresponse2 = await ts.CallProjectWebApi("api/v4/projectsettings", HttpMethod.Put, configJson2, customerUid.ToString(), RestClient.ANOTHER_JWT);
      var putobjresp2 = JsonConvert.DeserializeObject<ProjectSettingsResult>(putresponse2);

      tempSettings = JsonConvert.SerializeObject(putobjresp2.settings).Replace("\"", string.Empty);

      //Assert.Equal(projectSettings2, putobjresp2.setting);
      Assert.Equal(projectSettings2, tempSettings);
      Assert.Equal(ts.ProjectUid.ToString(), putobjresp2.projectUid);

      // get call
      var getresponse1 = await ts.CallProjectWebApi($"api/v4/projectsettings/{ts.ProjectUid}", HttpMethod.Get, null, customerUid.ToString());
      var getobjresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(getresponse1);

      tempSettings = JsonConvert.SerializeObject(getobjresp1.settings).Replace("\"", string.Empty);

      //Assert.Equal(projectSettings1, getobjresp1.settings);
      Assert.Equal(projectSettings1, tempSettings);
      Assert.Equal(ts.ProjectUid.ToString(), getobjresp1.projectUid);
    }

    [Fact]
    public async Task AddInvalidProjectSettings()
    {
      const string testText = "Project settings test 2";
      Msg.Title(testText, "Add project settings for a standard project with invalid project UID");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();

      ts.IsPublishToWebApi = true;
      var projectSettings = "{ Invalid project UID }";
      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, projectSettings, ProjectSettingsType.Targets);
      var configJson = JsonConvert.SerializeObject(projSettings, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = await ts.CallProjectWebApi("api/v4/projectsettings", HttpMethod.Put, configJson, customerUid.ToString(), statusCode: HttpStatusCode.BadRequest);
      Assert.True(response == "{\"Code\":2001,\"Message\":\"No access to the project for a customer or the project does not exist.\"}", "Actual response different to expected");
      // Try to get the project that doesn't exist
      var response1 = await ts.CallProjectWebApi($"api/v4/projectsettings/{projectUid}", HttpMethod.Get, null, customerUid.ToString(), statusCode: HttpStatusCode.BadRequest);
      Assert.True(response1 == "{\"Code\":2001,\"Message\":\"No access to the project for a customer or the project does not exist.\"}", "Actual response different to expected");
    }    

    [Fact]
    public async Task AddEmptyProjectSettings()
    {
      const string testText = "Project settings test 3";
      Msg.Title(testText, "Add project settings for a project monitoring project");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);
      
      // Now create the settings
      var projectSettings = string.Empty;
      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(ts.ProjectUid.ToString(), projectSettings, ProjectSettingsType.Targets);
      var configJson = JsonConvert.SerializeObject(projSettings, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response = await ts.CallProjectWebApi("api/v4/projectsettings", HttpMethod.Put, configJson, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<ProjectSettingsResult>(response);

      var tempSettings = objresp.settings == null ? string.Empty : JsonConvert.SerializeObject(objresp.settings).Replace("\"", string.Empty);

      Assert.Equal(projectSettings, tempSettings);
      //Assert.Equal(ts.ProjectUid, objresp.projectUid);

      // get call
      var response1 = await ts.CallProjectWebApi($"api/v4/projectsettings/{ts.ProjectUid}", HttpMethod.Get, null, customerUid.ToString());
      var objresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(response1);

      tempSettings = objresp1.settings == null ? string.Empty : JsonConvert.SerializeObject(objresp1.settings).Replace("\"", string.Empty);

      Assert.Equal(projectSettings, tempSettings);
      //Assert.Equal(ts.ProjectUid, objresp1.projectUid);
    }

    [Fact]
    public async Task AddProjectSettingsThenUpdateThem()
    {
      const string testText = "Project settings test 4";
      Msg.Title(testText, "Add project settings for a project monitoring project");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      // Now create the settings
      var projectSettings = "{useMachineTargetPassCount: false,customTargetPassCountMinimum: 5}";

      projectSettings = projectSettings.Replace(" ", string.Empty);

      var projSettings = ProjectSettingsRequest.CreateProjectSettingsRequest(ts.ProjectUid.ToString(), projectSettings, ProjectSettingsType.Targets);
      var configJson = JsonConvert.SerializeObject(projSettings, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      await ts.CallProjectWebApi("api/v4/projectsettings", HttpMethod.Put, configJson, customerUid.ToString());

      var projectSettings1 = "{customTargetPassCountMaximum: 7,useMachineTargetTemperature: false,customTargetTemperatureMinimum: 75}";

      projectSettings1 = projectSettings1.Replace(" ", string.Empty);

      var projSettings1 = ProjectSettingsRequest.CreateProjectSettingsRequest(ts.ProjectUid.ToString(), projectSettings1, ProjectSettingsType.Targets);
      var configJson2 = JsonConvert.SerializeObject(projSettings1, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var response1 = await ts.CallProjectWebApi("api/v4/projectsettings", HttpMethod.Put, configJson2, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<ProjectSettingsResult>(response1);

      var tempSettings = JsonConvert.SerializeObject(objresp.settings).Replace("\"", string.Empty);

      Assert.Equal(projectSettings1, tempSettings);
      Assert.Equal(ts.ProjectUid.ToString(), objresp.projectUid);

      // get call
      var response2 = await ts.CallProjectWebApi($"api/v4/projectsettings/{ts.ProjectUid}", HttpMethod.Get, null, customerUid.ToString());
      var objresp1 = JsonConvert.DeserializeObject<ProjectSettingsResult>(response2);

      tempSettings = JsonConvert.SerializeObject(objresp1.settings).Replace("\"", string.Empty);

      Assert.Equal(projectSettings1, tempSettings);
      Assert.Equal(ts.ProjectUid.ToString(), objresp1.projectUid);
    }   
  }
}

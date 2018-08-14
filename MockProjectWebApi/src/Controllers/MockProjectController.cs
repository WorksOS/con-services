using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using MockProjectWebApi.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockProjectController : Controller
  {
    /// <summary>
    /// Gets the list of projects used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The list of mocked projects</returns>
    [Route("api/v4/mockproject")]
    [HttpGet]
    public ProjectDataResult GetMockProjects()
    {
      Console.WriteLine("GetMockProjects");
      //var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      //Console.WriteLine("CustomerUID=" + customerUid + " and user=" + User);
      return new ProjectDataResult { ProjectDescriptors = this.projectList };
    }

    /// <summary>
    /// Gets the project settings targets used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The mocked settings</returns>
    [Route("api/v4/mock/projectsettings/{projectUid}")]
    [HttpGet]
    public ProjectSettingsDataResult GetMockProjectSettingsTargets(string projectUid)
    {
      Console.WriteLine($"GetMockProjectSettingsTargets: projectUid={projectUid}");

      JObject settings = null;

      if (projectUid == ConstantsUtil.CUSTOM_SETTINGS_DIMENSIONS_PROJECT_UID)
        settings = JsonConvert.DeserializeObject<JObject>(projectSettingsTargets);
      else if (projectUid == ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2)
        settings = JsonConvert.DeserializeObject<JObject>(projectSettingsTargetsEx);

      return new ProjectSettingsDataResult { ProjectUid = projectUid, Settings = settings };
    }

    /// <summary>
    /// Gets the project settings colours used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The mocked settings</returns>
    [Route("api/v4/mock/projectcolors/{projectUid}")]
    [HttpGet]
    public ProjectSettingsDataResult GetMockProjectSettingsColors(string projectUid)
    {
      Console.WriteLine($"GetMockProjectSettingsColors: projectUid={projectUid}");

      JObject settings = null;

      switch (projectUid)
      {
        case ConstantsUtil.CUSTOM_SETTINGS_DIMENSIONS_PROJECT_UID:
          {
            settings = JsonConvert.DeserializeObject<JObject>(projectSettingsColors);
            break;
          }
        case ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1:
          {
            settings = JsonResourceHelper.GetColorSettings(ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1);
            break;
          }
      }

      return new ProjectSettingsDataResult { ProjectUid = projectUid, Settings = settings };
    }

    private readonly string projectSettingsTargets = @"{
            customBulkingPercent: 6,
            customCutFillTolerances: [0.22, 0.11, 0.055, 0, -0.055, -0.11, -0.22],
            customPassCountTargets: [1,2,3,4,5,10,20,30],
            customShrinkagePercent: 3,
            customTargetCmv: 10,
            customTargetCmvPercentMaximum: 100,
            customTargetCmvPercentMinimum: 75,
            customTargetMdp: 145,
            customTargetMdpPercentMaximum: 100,
            customTargetMdpPercentMinimum: 90,
            customTargetPassCountMaximum: 3,
            customTargetPassCountMinimum: 2,
            customTargetSpeedMaximum: 11,
            customTargetSpeedMinimum: 7,
            customTargetTemperatureMaximum: 130,
            customTargetTemperatureMinimum: 75,
            useDefaultCutFillTolerances: false,
            useDefaultPassCountTargets: false,
            useDefaultTargetRangeCmvPercent: false,
            useDefaultTargetRangeMdpPercent: false,
            useDefaultTargetRangeSpeed: false,
            useDefaultVolumeShrinkageBulking: false,
            useMachineTargetCmv: false,
            useMachineTargetMdp: false,
            useMachineTargetPassCount: false,
            useMachineTargetTemperature: false
          }";

    private readonly string projectSettingsTargetsEx = @"{
            customBulkingPercent: 6,
            customCutFillTolerances: [0.22, 0.11, 0.055, 0, -0.055, -0.11, -0.22],
            customPassCountTargets: [1,2,3,4,5,10,20,30],
            customShrinkagePercent: 3,
            customTargetCmv: 10,
            customTargetCmvPercentMaximum: 100,
            customTargetCmvPercentMinimum: 75,
            customTargetMdp: 145,
            customTargetMdpPercentMaximum: 100,
            customTargetMdpPercentMinimum: 90,
            customTargetPassCountMaximum: 3,
            customTargetPassCountMinimum: 2,
            customTargetSpeedMaximum: 11,
            customTargetSpeedMinimum: 7,
            customTargetTemperatureMaximum: 130,
            customTargetTemperatureMinimum: 75,
            customCMVTargets: [0, 5, 15, 25, 35, 45, 55, 65, 75, 85, 95, 105, 115, 125, 135, 145],
            useDefaultCutFillTolerances: false,
            useDefaultPassCountTargets: false,
            useDefaultTargetRangeCmvPercent: false,
            useDefaultTargetRangeMdpPercent: false,
            useDefaultTargetRangeSpeed: false,
            useDefaultVolumeShrinkageBulking: false,
            useMachineTargetCmv: false,
            useMachineTargetMdp: false,
            useMachineTargetPassCount: false,
            useMachineTargetTemperature: false,
            useDefaultCMVTargets: false            
          }";
    private readonly string projectSettingsColors = @"{
            useDefaultElevationColors: false,
            elevationColors: [0xC80000, 0xFF0000, 0xFF3C00, 0xFF5A00, 0xFF8200, 0xFFAA00, 0xFFC800, 
                                0xFFDC00, 0xFAE600, 0xDCE600, 0xD2E600, 0xC8E600, 0xB4E600, 0x96E600, 
                                0x82E600, 0x64F000, 0x00FF00, 0x00F064, 0x00E682, 0x00E696, 0x00E6B4,
                                0x00E6C8, 0x00E6D2, 0x00DCDC, 0x00E6E6, 0x00C8E6, 0x00B4F0, 0x0096F5,
                                0x0078FA, 0x005AFF, 0x0000FF],
            useDefaultCMVDetailsColors: false,
            cmvDetailsColors: [0x01579B, 0x2473AE, 0x488FC1, 0x6BACD5, 0x8FC8E8, 0xB3E5FC, 0xDBECC8, 
                                0x99CB65, 0x649E38, 0x2D681D, 0xFFCCD2, 0xF6A3A8, 0xEE7A7E, 0xE55154,
                                0xDD282A, 0xD50000],
            useDefaultCMVSummaryColors: false,
            cmvOnTargetColor: 0x8BC34A,
            cmvOverTargetColor: 0xD50000,
            cmvUnderTargetColor: 0x1579B,
            useDefaultCMVPercentColors: false,
            cmvPercentColors: [0xD50000, 0xE57373, 0xFFCDD2, 0x8BC34A, 0xB3E5FC, 0x005AFF, 0x039BE5, 0x01579B],
            useDefaultPassCountDetailsColors: false,
            passCountDetailsColors: [0x2D5783, 0x439BDC, 0xBEDFF1, 0x9DCE67, 0x6BA03E, 0x3A6B25, 0xF6CED3, 0xD57A7C, 0xC13037],
            useDefaultPassCountSummaryColors: false,
            passCountOnTargetColor: 0x8BC34A,
            passCountOverTargetColor: 0xD50000,
            passCountUnderTargetColor: 0x1579B,
            useDefaultCutFillColors: false,
            cutFillColors: [0xD50000, 0xE57373, 0xFFCDD2, 0x8BC34A, 0xB3E5FC, 0x039BE5, 0x01579B],
            useDefaultTemperatureSummaryColors: false,
            temperatureOnTargetColor: 0x8BC34A,
            temperatureOverTargetColor: 0xD50000,
            temperatureUnderTargetColor: 0x1579B,
            useDefaultSpeedSummaryColors: false,
            speedOnTargetColor: 0x8BC34A,
            speedOverTargetColor: 0xD50000,
            speedUnderTargetColor: 0x1579B,
            useDefaultMDPSummaryColors: false,
            mdpOnTargetColor: 0x8BC34A,
            mdpOverTargetColor: 0xD50000,
            mdpUnderTargetColor: 0x1579B
          }";

    private readonly List<ProjectData> projectList = new List<ProjectData>
    {
      new ProjectData {LegacyProjectId = 1000001, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1000100, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1000102, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1000450, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1000452, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1000544, ProjectUid = "dc509939-88b5-49b6-8c2c-9e8131122e96"},
      new ProjectData {LegacyProjectId = 1000992, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1001151, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1001152, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1001153, ProjectUid = "b14bb927-3c10-47b2-b958-4ce7aabbc594", ProjectGeofenceWKT = "POLYGON((6.96461375644884 46.250301540882, 6.96643887353764 46.2509268520462, 6.97460415600528 46.2477169036207, 6.97269423208211 46.2470325441392, 6.96461375644884 46.250301540882))"},
      new ProjectData {
        LegacyProjectId = ConstantsUtil.DIMENSIONS_PROJECT_ID,
        ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
        ProjectTimeZone = "Pacific Standard Time",
        IanaTimeZone = "America/Los_Angeles",
        ProjectGeofenceWKT = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.017698488251 36.2102040420362, -115.025723657623 36.2101347890754))"
      },
      new ProjectData {
        LegacyProjectId = ConstantsUtil.DIMENSIONS_EMPTY_PROJECT_ID,
        ProjectUid = ConstantsUtil.DIMENSIONS_EMPTY_PROJECT_UID,
        ProjectTimeZone = "Pacific Standard Time",
        IanaTimeZone = "America/Los_Angeles",
        ProjectGeofenceWKT = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.017698488251 36.2102040420362, -115.025723657623 36.2101347890754))"
      },
      new ProjectData {
        LegacyProjectId = ConstantsUtil.CUSTOM_SETTINGS_DIMENSIONS_PROJECT_ID,
        ProjectUid = ConstantsUtil.CUSTOM_SETTINGS_DIMENSIONS_PROJECT_UID
      },
      new ProjectData {LegacyProjectId = 1001164, ProjectUid = "a2cb39c7-95a0-4bb1-845f-cb1052467e98", ProjectTimeZone = "W. Europe Standard Time"},
      new ProjectData {LegacyProjectId = 1001184, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1001186, ProjectUid = "8590b7fc-079e-4b5a-b5ff-8514dadfe985"},
      new ProjectData {LegacyProjectId = 1001191, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1001209, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1001210, ProjectUid = "d0a0410e-9fcc-44b1-bf1a-378c891d2ddb"},
      new ProjectData {LegacyProjectId = 1001214, ProjectUid = "8aed6003-b8eb-47b1-941f-096a17468bf0"},
      new ProjectData {LegacyProjectId = 1001276, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1001280, ProjectUid = "04c94921-6343-4ffb-9d35-db9d281743fc"},
      new ProjectData {LegacyProjectId = 1001285, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1001388, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1001544, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {
        LegacyProjectId = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_ID_1,
        ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        ProjectTimeZone = "Mountain Standard Time",
        IanaTimeZone = "America/Creston"
      },
      new ProjectData {LegacyProjectId = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_ID_2, ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2},
      new ProjectData {LegacyProjectId = 1009999, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1012413, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1099999, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1999999, ProjectUid = "0fa94210-0d7a-4015-9eee-4d9956f4b250"},
      new ProjectData {
        LegacyProjectId = ConstantsUtil.LANDFILL_PROJECT_ID,
        ProjectUid = ConstantsUtil.LANDFILL_PROJECT_UID,
        ProjectType = ProjectType.LandFill
      },
      new ProjectData {
        ProjectUid = "b7f4af55-2fdb-4878-b3d0-ce748d5dde08",
        ProjectType = ProjectType.Standard,
        IsArchived = true
      }
    };
  }
}

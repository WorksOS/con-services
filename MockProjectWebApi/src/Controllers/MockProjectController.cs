using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Utils;
using System;
using System.Collections.Generic;
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
    /// Gets the project settings used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The mocked settings</returns>
    [Route("api/v4/mock/projectsettings/{projectUid}")]
    [HttpGet]
    public ProjectSettingsDataResult GetMockProjectSettings(string projectUid)
    {
      Console.WriteLine("GetMockProjectSettings: projectUid={0}", projectUid);
      string settings = null;
      if (projectUid == ConstantsUtil.CUSTOM_SETTINGS_DIMENSIONS_PROJECT_UID)
      {
        settings = @"{
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
      }
      return new ProjectSettingsDataResult { ProjectUid = projectUid, Settings = settings };
    }


    private readonly List<ProjectData> projectList = new List<ProjectData>
    {
      new ProjectData {LegacyProjectId = 1000001, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1000100, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1000102, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1000450, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1000452, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1000544, ProjectUid = Guid.NewGuid().ToString()},
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
      new ProjectData {LegacyProjectId = 1001280, ProjectUid = Guid.NewGuid().ToString()},
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
      new ProjectData {LegacyProjectId = ConstantsUtil.LANDFILL_PROJECT_ID, ProjectUid = ConstantsUtil.LANDFILL_PROJECT_UID, ProjectType = ProjectType.LandFill}
    };
  }
}

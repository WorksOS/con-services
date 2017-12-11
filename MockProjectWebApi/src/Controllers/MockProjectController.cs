﻿using Microsoft.AspNetCore.Mvc;
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
    /// Gets the list of imported files used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The list of mocked imported files</returns>
    [Route("api/v4/mock/importedfiles")]
    [HttpGet]
    public FileDataResult GetMockImportedFiles([FromQuery] Guid projectUid)
    {
      Console.WriteLine("GetMockImportedFiles: projectUid={0}", projectUid);

      var projectUidStr = projectUid.ToString();
      List<FileData> fileList = null;
      if (projectUidStr == ConstantsUtil.DIMENSIONS_PROJECT_UID)
      {
        fileList = new List<FileData>
        {
          new FileData
          {
            Name = "CERA.bg.dxf",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "DxfTileAcceptanceTest",
            ImportedFileType = ImportedFileType.Linework,
            ImportedFileUid = "cfcd4c01-6fc8-45d5-872f-513a0f619f03",
            LegacyFileId = 1,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 18
          },
          new FileData
          {
            Name = "Marylands_Metric.dxf",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "DxfTileAcceptanceTest",
            ImportedFileType = ImportedFileType.Linework,
            ImportedFileUid = "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b",
            LegacyFileId = 2,
            IsActivated = true,
            MinZoomLevel = 18,
            MaxZoomLevel = 19
          },
          new FileData
          {
            Name = "Large Sites Road - Trimble Road.TTM",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "CutFillAcceptanceTest",
            ImportedFileType = ImportedFileType.DesignSurface,
            ImportedFileUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
            LegacyFileId = 3,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 20
          },
          new FileData
          {
            Name = "Large Sites Road - Trimble Road.TTM",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "CutFillAcceptanceTest",
            ImportedFileType = ImportedFileType.DesignSurface,
            ImportedFileUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
            LegacyFileId = 111,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 20
          },
          new FileData
          {
            Name = "Large Sites Road.svl",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "StationOffsetReportTest",
            ImportedFileType = ImportedFileType.Alignment,
            ImportedFileUid = "6ece671b-7959-4a14-86fa-6bfe6ef4dd62",
            LegacyFileId = 112,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 17
          },
          new FileData
          {
            Name = "Topcon Road - Topcon Phil.svl",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "StationOffsetReportTest",
            ImportedFileType = ImportedFileType.Alignment,
            ImportedFileUid = "c6662be1-0f94-4897-b9af-28aeeabcd09b",
            LegacyFileId = 113,
            IsActivated = true,
            MinZoomLevel = 16,
            MaxZoomLevel = 18
          },
          new FileData
          {
            Name = "Milling - Milling.svl",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "StationOffsetReportTest",
            ImportedFileType = ImportedFileType.Alignment,
            ImportedFileUid = "3ead0c55-1e1f-4d30-aaf8-873526a2ab82",
            LegacyFileId = 114,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 19
          }
        };
      }
      else if (projectUidStr == ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1 ||
               projectUidStr == ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2)
      {
        fileList = this.surveyedSurfacesFileList;
        foreach (var file in fileList)
        {
          file.ProjectUid = projectUidStr;
          file.IsActivated = projectUidStr == ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1;
        }
        if (projectUidStr == ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1)
        {
          fileList.AddRange(this.designSurfacesFileList);
        }
      }

      return new FileDataResult { ImportedFileDescriptors = fileList };
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

    private readonly List<FileData> surveyedSurfacesFileList = new List<FileData>
    {
      new FileData
      {
        Name = "Original Ground Survey - Dimensions 2012_2016-05-13T000202Z.TTM",
        ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "SurveyedSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.SurveyedSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 14177,
        IsActivated = true,
        MinZoomLevel = 0,
        MaxZoomLevel = 0
      },
      new FileData
      {
        Name = "Large Sites Road - Trimble Road_2016-05-13T000000Z.TTM",
        ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "SurveyedSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.SurveyedSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 14176,
        IsActivated = true,
        MinZoomLevel = 0,
        MaxZoomLevel = 0
      },
      new FileData
      {
        Name = "Milling - Milling_2016-05-08T234647Z.TTM",
        ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "SurveyedSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.SurveyedSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 14175,
        IsActivated = true,
        MinZoomLevel = 0,
        MaxZoomLevel = 0
      },
      new FileData
      {
        Name = "Large Sites Road - Trimble Road_2016-05-08T234455Z.TTM",
        ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "SurveyedSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.SurveyedSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 14174,
        IsActivated = true,
        MinZoomLevel = 0,
        MaxZoomLevel = 0
      },
      new FileData
      {
        Name = "Large Sites Road - Trimble Road_2012-06-01T015500Z.TTM",
        ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "SurveyedSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.SurveyedSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 14222,
        IsActivated = true,
        MinZoomLevel = 0,
        MaxZoomLevel = 0
      }
    };

    private readonly List<FileData> designSurfacesFileList = new List<FileData>
    {
      new FileData
      {
        Name = "Original Ground Survey - Dimensions 2012.TTM",
        ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "DesignSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.DesignSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 15177,
        IsActivated = true,
        MinZoomLevel = 15,
        MaxZoomLevel = 19
      },
      new FileData
      {
        Name = "Large Sites Road - Trimble Road.TTM",
        ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "DesignSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.DesignSurface,
        ImportedFileUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
        LegacyFileId = 15176,
        IsActivated = true,
        MinZoomLevel = 15,
        MaxZoomLevel = 18
      },
      new FileData
      {
        Name = "Milling - Milling.TTM",
        ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "DesignSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.DesignSurface,
        ImportedFileUid = "220e12e5-ce92-4645-8f01-1942a2d5a57f",
        LegacyFileId = 15175,
        IsActivated = true,
        MinZoomLevel = 16,
        MaxZoomLevel = 17
      },
      new FileData
      {
        Name = "Topcon Road - Topcon.TTM",
        ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "DesignSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.DesignSurface,
        ImportedFileUid = "ea97efb9-c0c4-4a7f-9eee-e2b0ef0b0916",
        LegacyFileId = 15174,
        IsActivated = true,
        MinZoomLevel = 16,
        MaxZoomLevel = 18
      },
      new FileData
      {
        Name = "Trimble Command Centre.TTM",
        ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "DesignSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.DesignSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 15222,
        IsActivated = true,
        MinZoomLevel = 19,
        MaxZoomLevel = 20
      }
    };

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

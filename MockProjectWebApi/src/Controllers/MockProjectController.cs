using System;
using System.Collections.Generic;
using MasterDataModels.Models;
using MasterDataModels.ResultHandling;
using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.MasterDataProxies.Models;
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
      return new ProjectDataResult { ProjectDescriptors = projectList};
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
      if (projectUidStr == DIMENSIONS_PROJECT_UID)
      {
        fileList = new List<FileData>
        {
          new FileData
          {
            Name = "CERA.bg.dxf",
            ProjectUid = DIMENSIONS_PROJECT_UID,
            CustomerUid = "DxfTileAcceptanceTest", 
            ImportedFileType = ImportedFileType.Linework,
            ImportedFileUid = "cfcd4c01-6fc8-45d5-872f-513a0f619f03",
            LegacyFileId = 1,
            IsActivated = true
          },
          new FileData
          {
            Name = "Marylands_Metric.dxf",
            ProjectUid = DIMENSIONS_PROJECT_UID,
            CustomerUid = "DxfTileAcceptanceTest", 
            ImportedFileType = ImportedFileType.Linework,
            ImportedFileUid = "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b",
            LegacyFileId = 2,
            IsActivated = true
          }
        };
      }
      else if (projectUidStr == GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1 ||
               projectUidStr == GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2)
      {
        fileList = surveyedSurfacesFileList;
        foreach (var file in fileList)
        {
          file.ProjectUid = projectUidStr;
          file.IsActivated = projectUidStr == GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1;
        }

        if (projectUidStr == GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1)
        {
          fileList.AddRange(designSurfacesFileList);
        }
      }

      return new FileDataResult { ImportedFileDescriptors = fileList};
    }

    private List<FileData> surveyedSurfacesFileList = new List<FileData>
    {
      new FileData
      {
        Name = "Original Ground Survey - Dimensions 2012_2016-05-13T000202Z.TTM",
        ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "SurveyedSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.SurveyedSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 14177,
        IsActivated = true
      },
      new FileData
      {
        Name = "Large Sites Road - Trimble Road_2016-05-13T000000Z.TTM",
        ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "SurveyedSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.SurveyedSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 14176,
        IsActivated = true
      },
      new FileData
      {
        Name = "Milling - Milling_2016-05-08T234647Z.TTM",
        ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "SurveyedSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.SurveyedSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 14175,
        IsActivated = true
      },
      new FileData
      {
        Name = "Large Sites Road - Trimble Road_2016-05-08T234455Z.TTM",
        ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "SurveyedSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.SurveyedSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 14174,
        IsActivated = true
      },
      new FileData
      {
        Name = "Large Sites Road - Trimble Road_2012-06-01T015500Z.TTM",
        ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "SurveyedSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.DesignSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 14222,
        IsActivated = true
      }
    };

    private List<FileData> designSurfacesFileList = new List<FileData>
    {
      new FileData
      {
        Name = "Original Ground Survey - Dimensions 2012.TTM",
        ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "DesignSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.DesignSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 15177,
        IsActivated = true
      },
      new FileData
      {
        Name = "Large Sites Road - Trimble Road.TTM",
        ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "DesignSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.DesignSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 15176,
        IsActivated = true
      },
      new FileData
      {
        Name = "Milling - Milling.TTM",
        ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "DesignSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.DesignSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 15175,
        IsActivated = true
      },
      new FileData
      {
        Name = "Topcon Road - Topcon.TTM",
        ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "DesignSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.DesignSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 15174,
        IsActivated = true
      },
      new FileData
      {
        Name = "Trimble Command Centre.TTM",
        ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
        CustomerUid = "DesignSurfaceAcceptanceTest",
        ImportedFileType = ImportedFileType.DesignSurface,
        ImportedFileUid = Guid.NewGuid().ToString(),
        LegacyFileId = 15222,
        IsActivated = true
      }
    };

    private List<ProjectData>  projectList = new List<ProjectData>
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
      new ProjectData {LegacyProjectId = DIMENSIONS_PROJECT_ID, ProjectUid = DIMENSIONS_PROJECT_UID},
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
      new ProjectData {LegacyProjectId = GOLDEN_DATA_DIMENSIONS_PROJECT_ID_1, ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1},
      new ProjectData {LegacyProjectId = GOLDEN_DATA_DIMENSIONS_PROJECT_ID_2, ProjectUid = GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2},
      new ProjectData {LegacyProjectId = 1009999, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1012413, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1099999, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1999999, ProjectUid = "0fa94210-0d7a-4015-9eee-4d9956f4b250"}
    };

    private const int DIMENSIONS_PROJECT_ID = 1001158;
    private const string DIMENSIONS_PROJECT_UID = "ff91dd40-1569-4765-a2bc-014321f76ace";

    private const int GOLDEN_DATA_DIMENSIONS_PROJECT_ID_1 = 1007777;
    private const string GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1 = "7925f179-013d-4aaf-aff4-7b9833bb06d6";
    private const int GOLDEN_DATA_DIMENSIONS_PROJECT_ID_2 = 1007778;
    private const string GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2 = "86a42bbf-9d0e-4079-850f-835496d715c5";
  }
}

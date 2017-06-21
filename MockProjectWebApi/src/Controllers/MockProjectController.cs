﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Models;
using src.Models;
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
    public MockProjectContractResult GetMockProjects()
    {
      Console.WriteLine("GetMockProjects");
      //var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      //Console.WriteLine("CustomerUID=" + customerUid + " and user=" + User);
      return new MockProjectContractResult {ProjectDescriptors = projectList};
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

      List<FileData> fileList = null;
      if (projectUid.ToString() == DIMENSIONS_PROJECT_UID)
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
            IsActivated = true
          },
          new FileData
          {
            Name = "Marylands_Metric.dxf",
            ProjectUid = DIMENSIONS_PROJECT_UID,
            CustomerUid = "DxfTileAcceptanceTest", 
            ImportedFileType = ImportedFileType.Linework,
            ImportedFileUid = "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b",
            IsActivated = true
          }
        };
      }
      return new FileDataResult { FileDescriptors = fileList};
    }

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
      new ProjectData {LegacyProjectId = 1009999, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1012413, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1099999, ProjectUid = Guid.NewGuid().ToString()},
      new ProjectData {LegacyProjectId = 1999999, ProjectUid = "0fa94210-0d7a-4015-9eee-4d9956f4b250"}
    };

    private const int DIMENSIONS_PROJECT_ID = 1001158;
    private const string DIMENSIONS_PROJECT_UID = "ff91dd40-1569-4765-a2bc-014321f76ace";
  }
}

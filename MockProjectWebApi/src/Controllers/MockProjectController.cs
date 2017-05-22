using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockProjectController : Controller
  {

    /// <summary>
    /// Gets the list of projects used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The list of mocked projects</returns>
    [Route("api/v3/mockproject")]
    [HttpGet]
    public List<ProjectData> GetMockProjects()
    {
      Console.WriteLine("GetMockProjects");
      //var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      //Console.WriteLine("CustomerUID=" + customerUid + " and user=" + User);
      var projectList = new List<ProjectData>
        {
          new ProjectData { LegacyProjectId = 1000001, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1000100, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1000102, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1000450, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1000452, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1000544, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1000992, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001151, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001152, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001153, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001158, ProjectUid = "ff91dd40-1569-4765-a2bc-014321f76ace" },
          new ProjectData { LegacyProjectId = 1001184, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001186, ProjectUid = "8590b7fc-079e-4b5a-b5ff-8514dadfe985" },
          new ProjectData { LegacyProjectId = 1001191, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001209, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001210, ProjectUid = "d0a0410e-9fcc-44b1-bf1a-378c891d2ddb" },
          new ProjectData { LegacyProjectId = 1001214, ProjectUid = "8aed6003-b8eb-47b1-941f-096a17468bf0" },
          new ProjectData { LegacyProjectId = 1001276, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001280, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001285, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001388, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001544, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1009999, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1012413, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1099999, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1999999, ProjectUid = "0fa94210-0d7a-4015-9eee-4d9956f4b250" }
        };
      return projectList;
    }
  }
}

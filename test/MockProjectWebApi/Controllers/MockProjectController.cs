using System;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using VSS.Raptor.Service.Common.Proxies.Models;

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
          new ProjectData { LegacyProjectId = 1001158, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001184, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001186, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001191, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001209, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001210, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001214, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001276, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001280, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001285, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001388, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1001544, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1009999, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1012413, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1099999, ProjectUid = Guid.NewGuid().ToString() },
          new ProjectData { LegacyProjectId = 1999999, ProjectUid = Guid.NewGuid().ToString() }
        };
      return projectList;
    }
  }
}

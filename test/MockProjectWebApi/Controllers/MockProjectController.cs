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
          new ProjectData { LegacyProjectId = 1000001 },
          new ProjectData { LegacyProjectId = 1000100 },
          new ProjectData { LegacyProjectId = 1000102 },
          new ProjectData { LegacyProjectId = 1000450 },
          new ProjectData { LegacyProjectId = 1000452 },
          new ProjectData { LegacyProjectId = 1000544 },
          new ProjectData { LegacyProjectId = 1000992 },
          new ProjectData { LegacyProjectId = 1001151 },
          new ProjectData { LegacyProjectId = 1001152 },
          new ProjectData { LegacyProjectId = 1001153 },
          new ProjectData { LegacyProjectId = 1001158 },
          new ProjectData { LegacyProjectId = 1001184 },
          new ProjectData { LegacyProjectId = 1001186 },
          new ProjectData { LegacyProjectId = 1001191 },
          new ProjectData { LegacyProjectId = 1001209 },
          new ProjectData { LegacyProjectId = 1001210 },
          new ProjectData { LegacyProjectId = 1001214 },
          new ProjectData { LegacyProjectId = 1001276 },
          new ProjectData { LegacyProjectId = 1001280 },
          new ProjectData { LegacyProjectId = 1001285 },
          new ProjectData { LegacyProjectId = 1001388 },
          new ProjectData { LegacyProjectId = 1001544 },
          new ProjectData { LegacyProjectId = 1009999 },
          new ProjectData { LegacyProjectId = 1012413 },
          new ProjectData { LegacyProjectId = 1099999 },
          new ProjectData { LegacyProjectId = 1999999 }
        };
      return projectList;
    }
  }
}

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Services;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockProjectController : BaseController
  {
    private readonly ProjectService projectService;

    public MockProjectController(ILoggerFactory loggerFactory, IProjectService projectService)
    : base(loggerFactory)
    {
      this.projectService = (ProjectService)projectService;
    }

    /// <summary>
    /// Gets the list of projects used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The list of mocked projects</returns>
    [Route("api/v6")]
    [Route("api/v6/project")]
    [HttpGet]
    public ProjectDataResult GetMockProjects()
    {
      Logger.LogInformation($"{nameof(GetMockProjects)}");
      //var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      //Logger.LogInformation("CustomerUID=" + customerUid + " and user=" + User);
      return new ProjectDataResult { ProjectDescriptors = projectService.ProjectList };
    }

    /// <summary>
    /// Gets the project used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The list of mocked projects</returns>
    [Route("api/v6/{projectUid}")]
    [Route("api/v6/project/{projectUid}")]
    [HttpGet]
    public ProjectDataSingleResult GetMockProject(Guid projectUid)
    {
      Logger.LogInformation($"{nameof(GetMockProject)}: projectUid={projectUid}");
      //var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      //Logger.LogInformation("CustomerUID=" + customerUid + " and user=" + User);
      return new ProjectDataSingleResult { ProjectDescriptor = projectService.ProjectList.SingleOrDefault(p => p.ProjectUID == projectUid.ToString()) };
    }   
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Services;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockProjectInternalController : BaseController
  {
    private readonly ProjectService projectService;

    public MockProjectInternalController(ILoggerFactory loggerFactory, IProjectService projectService)
    : base(loggerFactory)
    {
      this.projectService = (ProjectService)projectService;
    }

    /// <summary>
    /// Gets  projects for the customer
    /// The data is mocked.
    /// </summary>
    /// <returns>The list of mocked projects</returns>
    [Route("internal/v6/project/{customerUid}/projects")]
    [HttpGet]
    public ProjectDataResult GetMockProjects(Guid customerUid)
    {
      Logger.LogInformation($"{nameof(GetMockProjects)}: customerUid={customerUid}");
      return new ProjectDataResult()
      {
        ProjectDescriptors = new List<ProjectData>()
        { projectService.ProjectList.SingleOrDefault(p => p.CustomerUID == customerUid.ToString()) }
      };
    }

    /// <summary>
    /// Gets the project , even if it is archived
    /// The data is mocked.
    /// </summary>
    /// <returns>The list of mocked projects</returns>
    [Route("internal/v6/project/{projectUid}")]
    [HttpGet]
    public ProjectDataSingleResult GetMockProject(Guid projectUid)
    {
      Logger.LogInformation($"{nameof(GetMockProject)}: projectUid={projectUid}");
      return new ProjectDataSingleResult { ProjectDescriptor = projectService.ProjectList.SingleOrDefault(p => p.ProjectUID == projectUid.ToString()) };
    }

    /// <summary>
    /// Gets the project used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The list of mocked projects</returns>
    [HttpGet]
    [Route("internal/v6/project/shortId/{shortRaptorProjectId}")]
    public ProjectDataSingleResult GetMockProjectByShortId(int shortRaptorProjectId)
    {
      Logger.LogInformation($"{nameof(GetMockProjectByShortId)}: shortRaptorProjectId={shortRaptorProjectId}");
      return new ProjectDataSingleResult { ProjectDescriptor = projectService.ProjectList.SingleOrDefault(p => p.ShortRaptorProjectId == shortRaptorProjectId) };
    }

    /// <summary>
    /// Gets the list of projects used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The list of mocked projects</returns>
    [HttpGet]
    [Route("internal/v6/project/intersecting")]
    public ProjectDataResult GetMockIntersectingProjects(string customerUid,
       double latitude, double longitude, DateTime? timeOfPosition = null)
    {
      Logger.LogInformation($"{nameof(GetMockIntersectingProjects)}");
      return new ProjectDataResult { ProjectDescriptors = projectService.ProjectList };
    }  
  }
}

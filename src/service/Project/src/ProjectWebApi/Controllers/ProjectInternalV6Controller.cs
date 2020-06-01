using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project internal controller for use by services using application token e.g. TFA
  /// </summary>
  public class ProjectInternalV6Controller : BaseController<ProjectInternalV6Controller>
  {
    /// <summary>
    /// Gets or sets the httpContextAccessor.
    /// </summary>
    protected readonly IHttpContextAccessor HttpContextAccessor;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectInternalV6Controller(IHttpContextAccessor httpContextAccessor)
    {
      this.HttpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets a project in applicationContext i.e. no customer. 
    /// </summary>
    /// <returns>A project data</returns>
    [Route("internal/v6/project/{projectUid}")]
    [HttpGet]
    public async Task<ProjectV6DescriptorsSingleResult> GetProjectByUid(string projectUid)
    {
      Logger.LogInformation($"{nameof(GetProjectByUid)}");

      var project = await ProjectRequestHelper.GetProjectAndReturn(projectUid, Logger, ServiceExceptionHandler, CwsProjectClient, customHeaders).ConfigureAwait(false);
      return new ProjectV6DescriptorsSingleResult(AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project));
    }

    /// <summary>
    /// Gets intersecting projects in localDB . applicationContext i.e. no customer. 
    ///   if projectUid, get it if it overlaps in localDB
    ///    else get overlapping projects in localDB for this CustomerUID
    /// </summary>
    /// <returns>project data list</returns>
    [Route("internal/v6/project/intersecting")]
    [HttpGet]
    public async Task<ProjectV6DescriptorsListResult> GetIntersectingProjects(string customerUid,
      double latitude, double longitude, string projectUid)
    {
      Logger.LogInformation($"{nameof(GetIntersectingProjects)}");

      var projects = await ProjectRequestHelper.GetIntersectingProjects(
        customerUid, latitude, longitude, projectUid,
        Logger, ServiceExceptionHandler, CwsProjectClient, customHeaders).ConfigureAwait(false);
      return new ProjectV6DescriptorsListResult
      {
        ProjectDescriptors = projects.Select(project =>
            AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project))
          .ToImmutableList()
      };
    }
  }
}

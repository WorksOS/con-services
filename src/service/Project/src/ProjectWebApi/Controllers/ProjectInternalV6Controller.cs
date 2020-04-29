using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Push.Abstractions.Notifications;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project controller v6
  ///     requests and responses have changed IDs from Guids to strings
  ///     May be other changes
  /// </summary>
  public class ProjectInternalV6Controller : ProjectBaseController
  {
    /// <summary>
    /// Gets or sets the httpContextAccessor.
    /// </summary>
    protected readonly IHttpContextAccessor HttpContextAccessor;

    private readonly INotificationHubClient _notificationHubClient;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectInternalV6Controller(IHttpContextAccessor httpContextAccessor, INotificationHubClient notificationHubClient)
    {
      this.HttpContextAccessor = httpContextAccessor;
      this._notificationHubClient = notificationHubClient;
    }

    /// <summary>
    /// Gets intersecting projects in localDB . applicationContext i.e. no customer. 
    ///   if projectUid, get it if it overlaps inC:\CCSS\SourceCode\azure_C2S3CON-207\src\service\Project\src\ProjectWebApi\kestrelsettings.json localDB
    ///    else get overlapping projects in localDB for this CustomerUID
    /// </summary>
    /// <returns>project data list</returns>
    [Route("internal/v6/project/{customerUid}/projects")]
    [HttpGet]
    public async Task<ProjectV6DescriptorsListResult> GetProjects(string customerUid)
    {
      Logger.LogInformation($"{nameof(GetProjects)}");

      var projects = (await GetProjectListForCustomer(customerUid).ConfigureAwait(false)).ToImmutableList();

      return new ProjectV6DescriptorsListResult
      {
        ProjectDescriptors = projects.Select(project =>
            AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project))
          .ToImmutableList()
      };
    }

    /// <summary>
    /// Gets a project in applicationContext i.e. no customer. 
    /// </summary>
    /// <returns>A project data</returns>
    [Route("internal/v6/project/{projectUid}")]
    [Route("api/v6/project/applicationcontext/{projectUid}")] // todoJeannie obsolete once ProjectSvc changes merged to master
    [HttpGet]
    public async Task<ProjectV6DescriptorsSingleResult> GetProjectByUid(string projectUid)
    {
      Logger.LogInformation($"{nameof(GetProjectByUid)}");

      var project = await ProjectRequestHelper.GetProjectEvenIfArchived(projectUid.ToString(), Logger, ServiceExceptionHandler, ProjectRepo).ConfigureAwait(false);
      return new ProjectV6DescriptorsSingleResult(AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project));
    }

    /// <summary>
    /// Gets projects which this device has access to, from cws
    ///    application token i.e. customHeaders will NOT include customerUid
    ///    get this from localDB now.
    ///       response to include customerUid
    /// </summary>
    [Route("internal/v6/project/shortId/{shortRaptorProjectId}")]
    [Route("api/v6/project/applicationcontext/shortId/{shortRaptorProjectId}")] // todoJeannie obsolete once ProjectSvc changes merged to master
    [HttpGet]
    public async Task<ProjectV6DescriptorsSingleResult> GetProjectByShortId(long shortRaptorProjectId)
    {
      Logger.LogInformation($"{nameof(GetProjectByShortId)}");

      var project = await ProjectRequestHelper.GetProject(shortRaptorProjectId, Logger, ServiceExceptionHandler, ProjectRepo).ConfigureAwait(false);
      return new ProjectV6DescriptorsSingleResult(AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project));
    }

    /// <summary>
    /// Gets intersecting projects in localDB . applicationContext i.e. no customer. 
    ///   if projectUid, get it if it overlaps inC:\CCSS\SourceCode\azure_C2S3CON-207\src\service\Project\src\ProjectWebApi\kestrelsettings.json localDB
    ///    else get overlapping projects in localDB for this CustomerUID
    /// </summary>
    /// <returns>project data list</returns>
    [Route("internal/v6/project/intersecting")]
    [Route("api/v6/project/applicationcontext/intersecting")]  // todoJeannie obsolete once ProjectSvc changes merged to master
    [HttpGet]
    public async Task<ProjectV6DescriptorsListResult> GetIntersectingProjects(string customerUid,
       double latitude, double longitude)
    {
      Logger.LogInformation($"{nameof(GetIntersectingProjects)}");

      var projects = await ProjectRequestHelper.GetIntersectingProjects(
        customerUid, latitude, longitude,
        Logger, ServiceExceptionHandler, ProjectRepo).ConfigureAwait(false);
      return new ProjectV6DescriptorsListResult
      {
        ProjectDescriptors = projects.Select(project =>
            AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project))
           .ToImmutableList()
      };
    }

  }
}

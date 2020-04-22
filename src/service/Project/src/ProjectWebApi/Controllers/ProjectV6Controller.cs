using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project controller v6
  ///     requests and responses have changed IDs from Guids to strings
  ///     May be other changes
  /// </summary>
  public class ProjectV6Controller : ProjectBaseController
  {
    /// <summary>
    /// Gets or sets the httpContextAccessor.
    /// </summary>
    protected readonly IHttpContextAccessor HttpContextAccessor;

    private readonly INotificationHubClient notificationHubClient;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectV6Controller(IConfigurationStore configStore, IHttpContextAccessor httpContextAccessor, INotificationHubClient notificationHubClient)
      : base(configStore)
    {
      this.HttpContextAccessor = httpContextAccessor;
      this.notificationHubClient = notificationHubClient;
    }

    /// <summary>
    /// Gets a list of projects for a customer. The list includes projects of all project types
    ///        and both active and archived projects.
    /// </summary>
    [Route("api/v4/project")] // temporary kludge until ccssscon-219 
    [Route("api/v6/project")]
    [HttpGet]
    public async Task<ProjectV6DescriptorsListResult> GetProjectsV6()
    {
      Logger.LogInformation("GetAllProjectsV6");

      var projects = (await GetProjectList().ConfigureAwait(false)).ToImmutableList();

      return new ProjectV6DescriptorsListResult
      {
        ProjectDescriptors = projects.Select(project =>
            AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project))
          .ToImmutableList()
      };
    }

    /// <summary>
    /// Gets a project for a customer. 
    /// </summary>
    /// <returns>A project data</returns>
    [Route("api/v4/project/{projectUid}")]
    [Route("api/v6/project/{projectUid}")]
    [HttpGet]
    public async Task<ProjectV6DescriptorsSingleResult> GetProjectV6(string projectUid)
    {
      Logger.LogInformation("GetProjectV6");
     
      var project = await ProjectRequestHelper.GetProject(projectUid.ToString(), customerUid, Logger, ServiceExceptionHandler, ProjectRepo).ConfigureAwait(false);
      return new ProjectV6DescriptorsSingleResult(AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project));
    }

    /// <summary>
    /// Gets a project in applicationContext i.e. no customer. 
    /// </summary>
    /// <returns>A project data</returns>
    [Route("api/v4/project/applicationcontext/{projectUid}")]
    [Route("api/v6/project/applicationcontext/{projectUid}")]
    [HttpGet]
    public async Task<ProjectV6DescriptorsSingleResult> GetProjectUidApplicationContextV6(string projectUid)
    {
      Logger.LogInformation($"{nameof(GetProjectUidApplicationContextV6)}");

      var project = await ProjectRequestHelper.GetProjectEvenIfArchived(projectUid.ToString(), Logger, ServiceExceptionHandler, ProjectRepo).ConfigureAwait(false);
      return new ProjectV6DescriptorsSingleResult(AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project));
    }

    /// <summary>
    /// Gets projects which this device has access to, from cws
    ///    application token i.e. customHeaders will NOT include customerUid
    ///    get this from localDB now.
    ///       response to include customerUid
    /// </summary>
    [Route("api/v4/project/applicationcontext/shortId/{shortRaptorProjectId}")]
    [Route("api/v6/project/applicationcontext/shortId/{shortRaptorProjectId}")]
    [HttpGet]
    public async Task<ProjectV6DescriptorsSingleResult> GetProjectShortIdApplicationContextV6(long shortRaptorProjectId)
    {
      Logger.LogInformation($"{nameof(GetProjectShortIdApplicationContextV6)}");

      var project = await ProjectRequestHelper.GetProject(shortRaptorProjectId, Logger, ServiceExceptionHandler, ProjectRepo).ConfigureAwait(false);
      return new ProjectV6DescriptorsSingleResult(AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project));
    }

    /// <summary>
    /// Gets intersecting projects in localDB . applicationContext i.e. no customer. 
    ///   if projectUid, get it if it overlaps inC:\CCSS\SourceCode\azure_C2S3CON-207\src\service\Project\src\ProjectWebApi\kestrelsettings.json localDB
    ///    else get overlapping projects in localDB for this CustomerUID
    /// </summary>
    /// <returns>project data list</returns>
    [Route("api/v4/project/applicationcontext/intersecting")]
    [Route("api/v6/project/applicationcontext/intersecting")]
    [HttpGet]
    public async Task<ProjectV6DescriptorsListResult> GetIntersectingProjectsApplicationContextV6(string customerUid,
       double latitude, double longitude)
    {
      Logger.LogInformation($"{nameof(GetIntersectingProjectsApplicationContextV6)}");

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

    // POST: api/project
    /// <summary>
    /// Create Project
    ///    as of v6 this creates a project which includes the CustomerUID
    ///       Both the ProjectUID and CustomerUID are trns which come from ProfileX
    /// </summary>
    /// <param name="projectRequest">CreateProjectRequest model</param>
    /// <remarks>Create new project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("internal/v4/project")]
    [Route("api/v4/project")]
    [Route("internal/v6/project")]
    [Route("api/v6/project")]
    [HttpPost]
    public async Task<ProjectV6DescriptorsSingleResult> CreateProject([FromBody] CreateProjectRequest projectRequest)
    {
      if (projectRequest == null)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 39);

      Logger.LogInformation($"{nameof(CreateProject)} projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));

      if (projectRequest.CustomerUID == null) projectRequest.CustomerUID = new Guid(customerUid);
   
      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(projectRequest);
      createProjectEvent.ActionUTC = DateTime.UtcNow;
      ProjectDataValidator.Validate(createProjectEvent, ProjectRepo, ServiceExceptionHandler);
      if (createProjectEvent.CustomerUID.ToString() != customerUid)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 18);

      // ProjectUID won't be filled yet
      await ProjectDataValidator.ValidateProjectName(customerUid, createProjectEvent.ProjectName, createProjectEvent.ProjectUID.ToString(), Logger, ServiceExceptionHandler, ProjectRepo);

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            customerUid, userId, null, customHeaders,
            productivity3dV1ProxyCoord: Productivity3dV1ProxyCoord,
            projectRepo: ProjectRepo, fileRepo: FileRepo,
            dataOceanClient: DataOceanClient, authn: Authorization,
            cwsProjectClient: CwsProjectClient)
          .ProcessAsync(createProjectEvent)
      );

      var result = new ProjectV6DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(await ProjectRequestHelper.GetProject(createProjectEvent.ProjectUID.ToString(), customerUid, Logger, ServiceExceptionHandler, ProjectRepo)
          .ConfigureAwait(false)));

      await notificationHubClient.Notify(new CustomerChangedNotification(projectRequest.CustomerUID.Value));

      Logger.LogResult(this.ToString(), JsonConvert.SerializeObject(projectRequest), result);
      return result;
    }

    /// <summary>
    /// Create a scheduler job to create a project using internal urls 
    /// </summary>
    /// <param name="projectRequest">The project request model to be used</param>
    /// <param name="scheduler">The scheduler used to queue the job</param>
    /// <returns>Scheduler Job Result, containing the Job ID To poll via the Scheduler</returns>
    [Route("api/v4/project/background")]
    [Route("api/v6/project/background")]
    [HttpPost]
    public async Task<ScheduleJobResult> RequestCreateProjectBackgroundJob([FromBody] CreateProjectRequest projectRequest, [FromServices] ISchedulerProxy scheduler)
    {
      if (projectRequest == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 39);
      }
      
      var baseUrl = Request.Host.ToUriComponent(); 
      var callbackUrl = $"http://{baseUrl}/internal/v6/project";  
      Logger.LogInformation($"nameof(RequestCreateProjectBackgroundJob): baseUrl {callbackUrl}");

      var request = new ScheduleJobRequest
      {
        Filename = projectRequest.ProjectName + Guid.NewGuid(), // Make sure the filename is unique, it's not important what it's called as the scheduled job keeps a reference
        Method = "POST",
        Url = callbackUrl,
        Headers =
        {
          ["Content-Type"] = Request.Headers["Content-Type"]
        }
      };
      request.SetBinaryPayload(Request.Body);

      var customHeaders = Request.Headers.GetCustomHeaders();

      return await scheduler.ScheduleBackgroundJob(request, customHeaders);
    }

    // PUT: api/v6/project
    /// <summary>
    /// Update Project
    /// </summary>
    /// <param name="projectRequest">UpdateProjectRequest model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("internal/v4/project")]
    [Route("api/v4/project")]
    [Route("internal/v6/project")]
    [Route("api/v6/project")]
    [HttpPut]
    public async Task<ProjectV6DescriptorsSingleResult> UpdateProjectV6([FromBody] UpdateProjectRequest projectRequest)
    {
      if (projectRequest == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 40);
      }

      Logger.LogInformation("UpdateProjectV6. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));
      var project = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(projectRequest);      
      project.ActionUTC = DateTime.UtcNow;

      // validation includes check that project must exist - otherwise there will be a null legacyID.
      ProjectDataValidator.Validate(project, ProjectRepo, ServiceExceptionHandler);
      await ProjectDataValidator.ValidateProjectName(customerUid, projectRequest.ProjectName, projectRequest.ProjectUid.ToString(), Logger, ServiceExceptionHandler, ProjectRepo);

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpdateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            customerUid, userId, null, customHeaders,
            productivity3dV1ProxyCoord: Productivity3dV1ProxyCoord,
            projectRepo: ProjectRepo, fileRepo: FileRepo, httpContextAccessor: HttpContextAccessor,
            dataOceanClient: DataOceanClient, authn: Authorization, cwsProjectClient: CwsProjectClient)
          .ProcessAsync(project)
      );

      //invalidate cache in TRex/Raptor
      Logger.LogInformation("UpdateProjectV6. Invalidating 3D PM cache");
      await notificationHubClient.Notify(new ProjectChangedNotification(project.ProjectUID));

      Logger.LogInformation("UpdateProjectV6. Completed successfully");
      return new ProjectV6DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(await ProjectRequestHelper.GetProject(project.ProjectUID.ToString(), customerUid, Logger, ServiceExceptionHandler, ProjectRepo)
          .ConfigureAwait(false)));
    }

    /// <summary>
    /// Create a scheduler job to update an existing project in the background
    /// </summary>
    /// <param name="projectRequest">The project request model to be used in the update</param>
    /// <param name="scheduler">The scheduler used to queue the job</param>
    /// <returns>Scheduler Job Result, containing the Job ID To poll via the Scheduler</returns>
    [Route("api/v4/project/background")]
    [Route("api/v6/project/background")]
    [HttpPut]
    public async Task<ScheduleJobResult> RequestUpdateProjectBackgroundJob([FromBody] UpdateProjectRequest projectRequest, [FromServices] ISchedulerProxy scheduler)
    {
      if (projectRequest == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 39);
      }

      // do a quick validation to make sure the project acctually exists (this will also be run in the background task, but a quick response to the UI will be better if the project can't be updated)
      var project = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(projectRequest);
      project.ActionUTC = DateTime.UtcNow;
      // validation includes check that project must exist - otherwise there will be a null legacyID.
      ProjectDataValidator.Validate(project, ProjectRepo, ServiceExceptionHandler);
      await ProjectDataValidator.ValidateProjectName(customerUid, projectRequest.ProjectName, projectRequest.ProjectUid.ToString(), Logger, ServiceExceptionHandler, ProjectRepo);

      var baseUrl = Request.Host.ToUriComponent();
      var callbackUrl = $"http://{baseUrl}/internal/v6/project";
      Logger.LogInformation($"nameof(RequestUpdateProjectBackgroundJob): baseUrl {callbackUrl}");

      var request = new ScheduleJobRequest
      {
        Filename = projectRequest.ProjectName + Guid.NewGuid(), // Make sure the filename is unique, it's not important what it's called as the scheduled job keeps a reference
        Method = "PUT",
        Url = callbackUrl,
        Headers =
        {
          ["Content-Type"] = Request.Headers["Content-Type"]
        }
      };
      request.SetBinaryPayload(Request.Body);

      var customHeaders = Request.Headers.GetCustomHeaders();

      return await scheduler.ScheduleBackgroundJob(request, customHeaders);
    }

    // Archive: api/Project/
    /// <summary>
    /// Delete Project
    /// </summary>
    /// <param name="projectUid">projectUid to delete</param>
    /// <remarks>Deletes existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/project/{projectUid}")]
    [Route("api/v6/project/{projectUid}")]
    [HttpDelete]
    public async Task<ProjectV6DescriptorsSingleResult> ArchiveProjectV6([FromRoute] string projectUid)
    {
      LogCustomerDetails("ArchiveProjectV6", projectUid);
      var project = new DeleteProjectEvent
      {
        ProjectUID = new Guid(projectUid),
        DeletePermanently = false,
        ActionUTC = DateTime.UtcNow
      };
      ProjectDataValidator.Validate(project, ProjectRepo, ServiceExceptionHandler);

      var messagePayload = JsonConvert.SerializeObject(new { DeleteProjectEvent = project });
      var isDeleted = await ProjectRepo.StoreEvent(project).ConfigureAwait(false);
      if (isDeleted == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 66);

      // CCSSSCON-144 and CCSSSCON-32 call new archive endpoint in cws

      if (!string.IsNullOrEmpty(customerUid))
        await notificationHubClient.Notify(new CustomerChangedNotification(new Guid(customerUid)));

      Logger.LogInformation("ArchiveProjectV6. Completed successfully");
      return new ProjectV6DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(await ProjectRequestHelper.GetProject(project.ProjectUID.ToString(), customerUid, Logger, ServiceExceptionHandler, ProjectRepo)
          .ConfigureAwait(false)));

    }
    
  }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project controller v6
  ///    UI interface for projects i.e. user context
  /// </summary>
  public class ProjectV6Controller : BaseController<ProjectV6Controller>
  {
    /// <summary>
    /// Gets or sets the httpContextAccessor.
    /// </summary>
    protected readonly IHttpContextAccessor HttpContextAccessor;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectV6Controller(IHttpContextAccessor httpContextAccessor)
    {
      HttpContextAccessor = httpContextAccessor;
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
      Logger.LogInformation($"{nameof(GetProjectsV6)}");
      var projects = await ProjectRequestHelper.GetProjectListForCustomer(new Guid(CustomerUid), new Guid(UserId), Logger, ServiceExceptionHandler, CwsProjectClient, customHeaders);

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
      Logger.LogInformation($"{nameof(GetProjectV6)}");

      var project = await ProjectRequestHelper.GetProject(new Guid(projectUid), new Guid(CustomerUid), new Guid(UserId), Logger, ServiceExceptionHandler, CwsProjectClient, customHeaders).ConfigureAwait(false);
      return new ProjectV6DescriptorsSingleResult(AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project));
    }

    // POST: api/project
    /// <summary>
    /// Create a new Project.
    /// As of v6 this creates a project which includes the CustomerUID.
    /// Both the ProjectUID and CustomerUID are TRNs provided by ProfileX
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
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest projectRequest)
    {
      if (projectRequest == null)
        return BadRequest(ServiceExceptionHandler.CreateServiceError(HttpStatusCode.InternalServerError, 39));

      Logger.LogInformation($"{nameof(CreateProject)} projectRequest: {JsonConvert.SerializeObject(projectRequest)}");

      projectRequest.CustomerUID ??= new Guid(CustomerUid);
      if (projectRequest.CustomerUID.ToString() != CustomerUid)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 18);

      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(projectRequest);
      var validationResult
        = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<ValidateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserId, null, customHeaders,
            Productivity3dV1ProxyCoord, cwsProjectClient: CwsProjectClient)
          .ProcessAsync(data)
      );
      if (validationResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, validationResult.Code);

      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(projectRequest);
      createProjectEvent.ActionUTC = DateTime.UtcNow;

      var result = (await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserId, null, customHeaders,
            Productivity3dV1ProxyCoord, fileRepo: FileRepo,
            dataOceanClient: DataOceanClient, authn: Authorization,
            cwsProjectClient: CwsProjectClient, cwsDesignClient: CwsDesignClient,
            cwsProfileSettingsClient: CwsProfileSettingsClient)
          .ProcessAsync(createProjectEvent)) as ProjectV6DescriptorsSingleResult
        );
    
      await NotificationHubClient.Notify(new CustomerChangedNotification(projectRequest.CustomerUID.Value));

      Logger.LogResult(ToString(), JsonConvert.SerializeObject(projectRequest), result);
      return Ok(result);
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
    public async Task<IActionResult> RequestCreateProjectBackgroundJob([FromBody] CreateProjectRequest projectRequest, [FromServices] ISchedulerProxy scheduler)
    {
      if (projectRequest == null)
      {
        return BadRequest(ServiceExceptionHandler.CreateServiceError(HttpStatusCode.InternalServerError, 39));
      }

      var baseUrl = Request.Host.ToUriComponent();
      var callbackUrl = $"http://{baseUrl}/internal/v6/project";
      Logger.LogInformation($"{nameof(RequestCreateProjectBackgroundJob)}: baseUrl {callbackUrl}");

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

      return Ok(await scheduler.ScheduleBackgroundJob(request, Request.Headers.GetCustomHeaders()));
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
    public async Task<IActionResult> UpdateProjectV6([FromBody] UpdateProjectRequest projectRequest)
    {
      if (projectRequest == null)
        return BadRequest(ServiceExceptionHandler.CreateServiceError(HttpStatusCode.InternalServerError, 40));
      Logger.LogInformation($"{nameof(UpdateProjectV6)}: projectRequest: {JsonConvert.SerializeObject(projectRequest)}");

      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(projectRequest);
      data.CustomerUid = new Guid(CustomerUid);
      var validationResult
        = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<ValidateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
              CustomerUid, UserId, null, customHeaders,
              Productivity3dV1ProxyCoord, cwsProjectClient: CwsProjectClient)
            .ProcessAsync(data)
        );
      if (validationResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, validationResult.Code);


      var project = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(projectRequest);
      project.ActionUTC = DateTime.UtcNow;

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpdateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserId, null, customHeaders,
            Productivity3dV1ProxyCoord,
            fileRepo: FileRepo, httpContextAccessor: HttpContextAccessor,
            dataOceanClient: DataOceanClient, authn: Authorization,
            cwsProjectClient: CwsProjectClient, cwsDesignClient: CwsDesignClient, cwsProfileSettingsClient: CwsProfileSettingsClient)
          .ProcessAsync(project)
      );

      //invalidate cache in TRex/Raptor
      Logger.LogInformation($"{nameof(UpdateProjectV6)}: Invalidating 3D PM cache");
      await NotificationHubClient.Notify(new ProjectChangedNotification(project.ProjectUID));

      Logger.LogInformation($"{nameof(UpdateProjectV6)} Completed successfully");
      var result = new ProjectV6DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(await ProjectRequestHelper.GetProject(project.ProjectUID, new Guid(CustomerUid), new Guid(UserId), Logger, ServiceExceptionHandler, CwsProjectClient, customHeaders)
          .ConfigureAwait(false)));

      return Ok(result);
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
    public async Task<IActionResult> RequestUpdateProjectBackgroundJob([FromBody] UpdateProjectRequest projectRequest, [FromServices] ISchedulerProxy scheduler)
    {
      if (projectRequest == null)
        return BadRequest(ServiceExceptionHandler.CreateServiceError(HttpStatusCode.InternalServerError, 39));

      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(projectRequest);
      data.CustomerUid = new Guid(CustomerUid);
      var validationResult
        = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<ValidateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
              CustomerUid, UserId, null, customHeaders,
              Productivity3dV1ProxyCoord, fileRepo: FileRepo,
              dataOceanClient: DataOceanClient, authn: Authorization,
              cwsProjectClient: CwsProjectClient, cwsDesignClient: CwsDesignClient,
              cwsProfileSettingsClient: CwsProfileSettingsClient)
            .ProcessAsync(data)
        );
      if (validationResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, validationResult.Code);

      // do a quick validation to make sure the project acctually exists (this will also be run in the background task, but a quick response to the UI will be better if the project can't be updated)
      var project = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(projectRequest);
      project.ActionUTC = DateTime.UtcNow;

      var baseUrl = Request.Host.ToUriComponent();
      var callbackUrl = $"http://{baseUrl}/internal/v6/project";
      Logger.LogInformation($"{nameof(RequestUpdateProjectBackgroundJob)}: baseUrl {callbackUrl}");

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

      return Ok(await scheduler.ScheduleBackgroundJob(request, Request.Headers.GetCustomHeaders()));
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
      var project = new DeleteProjectEvent { ProjectUID = new Guid(projectUid), DeletePermanently = false, ActionUTC = DateTime.UtcNow };

      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(project);
      data.CustomerUid = new Guid(CustomerUid);
      var validationResult
        = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<ValidateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
              CustomerUid, UserId, null, customHeaders,
              Productivity3dV1ProxyCoord, cwsProjectClient: CwsProjectClient)
            .ProcessAsync(data)
        );
      if (validationResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, validationResult.Code);

      // CCSSSCON-144 and CCSSSCON-32 call new archive endpoint in cws
      var isDeleted = 1;
      //isDeleted = await ProjectRepo.StoreEvent(project).ConfigureAwait(false);
      if (isDeleted == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 66);

      if (!string.IsNullOrEmpty(CustomerUid))
        await NotificationHubClient.Notify(new CustomerChangedNotification(new Guid(CustomerUid)));

      Logger.LogInformation($"{nameof(ArchiveProjectV6)}: Completed successfully");
      return new ProjectV6DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(await ProjectRequestHelper.GetProject(project.ProjectUID, new Guid(CustomerUid), new Guid(UserId), Logger, ServiceExceptionHandler, CwsProjectClient, customHeaders)
          .ConfigureAwait(false)));
    }

    /// <summary>
    /// Called from CWS before a create or update can go ahead, returns pass or fail
    /// </summary>
    /// <param name="validateDto">Update Model</param>
    /// <returns></returns>
    [HttpPost("api/v6/project/validate")]
    public async Task<ContractExecutionResult> IsProjectValid([FromBody]ProjectValidateDto validateDto)
    {
      Logger.LogInformation($"{nameof(IsProjectValid)} Project Validation Check {JsonConvert.SerializeObject(validateDto)}");

      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(validateDto);
      var result = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<ValidateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserId, null, customHeaders,
            Productivity3dV1ProxyCoord, fileRepo: FileRepo,
            dataOceanClient: DataOceanClient, authn: Authorization,
            cwsProjectClient: CwsProjectClient, cwsDesignClient: CwsDesignClient,
            cwsProfileSettingsClient: CwsProfileSettingsClient)
          .ProcessAsync(data)
      );
      return result;
    }

    /// <summary>
    /// Called from CWS when a project or Coordinate System has been created / updated / deleted
    /// </summary>
    /// <param name="updateDto">Update Model</param>
    /// <returns></returns>
    [HttpPost("api/v6/project/notifychange")]
    public async Task<IActionResult> OnProjectChangeNotify([FromBody]ProjectChangeNotificationDto updateDto)
    {
      Logger.LogInformation($"{nameof(OnProjectChangeNotify)} Update Notification {JsonConvert.SerializeObject(updateDto)}");

      var projectGuid = string.IsNullOrEmpty(updateDto.ProjectTrn) ? null : TRNHelper.ExtractGuid(updateDto.ProjectTrn);
      if (projectGuid.HasValue)
      {
        Logger.LogInformation($"Clearing cache related to Project ID: {projectGuid.Value}");
        await NotificationHubClient.Notify(new ProjectChangedNotification(projectGuid.Value));
      }

      var accountGuid = string.IsNullOrEmpty(updateDto.AccountTrn) ? null : TRNHelper.ExtractGuid(updateDto.AccountTrn);
      if (accountGuid.HasValue)
      {
        Logger.LogInformation($"Clearing cache related to Project ID: {accountGuid.Value}");
        await NotificationHubClient.Notify(new CustomerChangedNotification(accountGuid.Value));
      }

      Logger.LogInformation($"{nameof(OnProjectChangeNotify)} Processed Notification");

      return Ok();
    }

    /// <summary>
    /// Used for CWS to notify us on association changes, so we can update cache
    /// </summary>
    /// <param name="trns">A list of TRNs that have been changed</param>
    /// <returns>Ok</returns>
    [HttpPost("api/v1/project/notifyassociation")]
    public async Task<IActionResult> OnProjectAssociationChange([FromBody] List<string> trns)
    {
      Logger.LogInformation($"{nameof(OnProjectAssociationChange)} Associations Updated: {JsonConvert.SerializeObject(trns)}");

      // We don't actually do anything with this data yet, other than clear cache
      // Since we call out to CWS for data
      var tasks = new Task[trns.Count];
      for (var i = 0; i < trns.Count; i++)
      {
        var trn = trns[i];
        var guid = TRNHelper.ExtractGuid(trn);
        if (!guid.HasValue)
          continue;

        Logger.LogInformation($"Clearing cache related to TRN: {guid.Value}");
        tasks[i] = NotificationHubClient.Notify(new ProjectChangedNotification(guid.Value));
      }

      await Task.WhenAll(tasks);

      return Ok();
    }

  }
}

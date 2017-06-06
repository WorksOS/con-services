using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Extensions.Logging;
using KafkaConsumer.Kafka;
using MasterDataProxies;
using MasterDataProxies.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectWebApiCommon.Models;
using ProjectWebApiCommon.ResultsHandling;
using ProjectWebApiCommon.Utilities;
using Repositories;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace Controllers
{
  /// <summary>
  /// Project controller v4
  /// </summary>
  public class ProjectV4Controller : ProjectBaseController
  {
    public ProjectV4Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ILoggerFactory logger)
      : base(producer, projectRepo, subscriptionsRepo, store, subsProxy, geofenceProxy, raptorProxy, logger)
    {
    }

    #region projects

    /// <summary>
    /// Gets a list of projects for a customer. The list includes projects of all project types
    /// and both active and archived projects.
    /// </summary>
    /// <returns>A list of projects</returns>
    [Route("api/v4/project")]
    [HttpGet]
    public async Task<ProjectV4DescriptorsListResult> GetProjectsV4()
    {
      log.LogInformation("GetProjectsV4");

      var projects = await GetProjectList().ConfigureAwait(false);
      return new ProjectV4DescriptorsListResult()
      {
        ProjectDescriptors = projects.Select(project =>
            AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(project))
          .ToImmutableList()
      };
    }

    /// <summary>
    /// Gets a project for a customer. 
    /// </summary>
    /// <returns>A project data</returns>
    [Route("api/v4/project/{projectUid}")]
    [HttpGet]
    public async Task<ProjectV4DescriptorsSingleResult> GetProjectV4(string projectUid)
    {
      log.LogInformation("GetProjectV4");
      var project = await GetProject(projectUid).ConfigureAwait(false);
      return new ProjectV4DescriptorsSingleResult(AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(project));
    }

    // POST: api/project
    /// <summary>
    /// Create Project
    ///    as of v4 this creates a project AND the association to Customer
    /// </summary>
    /// <param name="projectRequest">CreateProjectRequest model</param>
    /// <remarks>Create new project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/project")]
    [HttpPost]
    public async Task<ProjectV4DescriptorsSingleResult> CreateProjectV4([FromBody] CreateProjectRequest projectRequest)
    {
      var customerUid = Guid.Parse(((User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType);
      if (projectRequest == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(39),
            contractExecutionStatesEnum.FirstNameWithOffset(39)));
      }

      log.LogInformation("CreateProjectV4. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));

      if (projectRequest.CustomerUID == null) projectRequest.CustomerUID = customerUid;
      if (projectRequest.ProjectUID == null) projectRequest.ProjectUID = Guid.NewGuid();

      var project = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(projectRequest);
      project.ReceivedUTC = project.ActionUTC = DateTime.UtcNow;
      ProjectDataValidator.Validate(project, projectService);
      if (project.CustomerUID != customerUid)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(18),
            contractExecutionStatesEnum.FirstNameWithOffset(18)));
      }

      await ValidateCoordSystem(project).ConfigureAwait(false);
      ProjectBoundaryValidator.ValidateWKT(project.ProjectBoundary);

      log.LogDebug($"Testing if there are overlapping projects for project {project.ProjectName}");
      await DoesProjectOverlap(project, project.ProjectBoundary);

      // validate projectCustomer
      AssociateProjectCustomer customerProject = new AssociateProjectCustomer()
      {
        CustomerUID = project.CustomerUID,
        LegacyCustomerID = project.CustomerID,
        ProjectUID = project.ProjectUID,
        RelationType = RelationType.Owner,
        ActionUTC = project.ActionUTC,
        ReceivedUTC = project.ReceivedUTC
      };

      ProjectDataValidator.Validate(customerProject, projectService);
      customerProject.ReceivedUTC = DateTime.UtcNow;

      await ValidateAssociateSubscriptions(project).ConfigureAwait(false);

      project = await CreateProject(project, customerProject).ConfigureAwait(false);

      var userUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).Name;
      log.LogDebug($"Creating a geofence for project {project.ProjectName}");
      await geofenceProxy.CreateGeofence(project.CustomerUID, project.ProjectName, "", "", project.ProjectBoundary,
        0, true, Guid.Parse(userUid), Request.Headers.GetCustomHeaders()).ConfigureAwait(false);

      // do this a late as possible in case something fails. We can cleanup kafka que.
      await CreateKafkaEvents(project, customerProject);

      log.LogDebug("CreateProjectV4. completed succesfully");
      return new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await GetProject(project.ProjectUID.ToString())
          .ConfigureAwait(false)));
    }

    // PUT: api/v4/project
    /// <summary>
    /// Update Project
    /// </summary>
    /// <param name="projectRequest">UpdateProjectRequest model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/project")]
    [HttpPut]
    public async Task<ProjectV4DescriptorsSingleResult> UpdateProjectV4([FromBody] UpdateProjectRequest projectRequest)
    {
      if (projectRequest == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(40),
            contractExecutionStatesEnum.FirstNameWithOffset(40)));
      }
      log.LogInformation("UpdateProjectV4. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));
      var project = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(projectRequest);
      project.ReceivedUTC = project.ActionUTC = DateTime.UtcNow;

      // validation includes check that project must exist - otherwise there will be a null legacyID.
      ProjectDataValidator.Validate(project, projectService);
      await ValidateCoordSystem(project).ConfigureAwait(false);

      if (!string.IsNullOrEmpty(project.CoordinateSystemFileName))
      {
        var projectWithLegacyProjectID = projectService.GetProjectOnly(project.ProjectUID.ToString()).Result;
        var customHeaders = (Request.Headers.GetCustomHeaders());
        customHeaders.Add("X-VisionLink-ClearCache", "true");
        var coordinateSystemSettingsResult = await raptorProxy
          .CoordinateSystemPost(projectWithLegacyProjectID.LegacyProjectID, project.CoordinateSystemFileContent,
            project.CoordinateSystemFileName, customHeaders).ConfigureAwait(false);
        log.LogDebug($"Post of CS update to RaptorServices returned code: {0} Message {1}.",
          coordinateSystemSettingsResult?.Code ?? -1,
          coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null");
        if (coordinateSystemSettingsResult == null ||
            coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
        {
          log.LogError($"Post of CS update to RaptorServices failed. Reason: {0} {1}.",
            coordinateSystemSettingsResult?.Code ?? -1,
            coordinateSystemSettingsResult?.Message ?? "null");

          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(41),
              string.Format(contractExecutionStatesEnum.FirstNameWithOffset(41),
                coordinateSystemSettingsResult?.Code ?? -1,
                coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null"
              )));
        }
      }

      var messagePayload = JsonConvert.SerializeObject(new {UpdateProjectEvent = project});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
        });
      await projectService.StoreEvent(project).ConfigureAwait(false);

      log.LogInformation("UpdateProjectV4. Completed successfully");
      return new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await GetProject(project.ProjectUID.ToString())
          .ConfigureAwait(false)));
    }


    // DELETE: api/Project/
    /// <summary>
    /// Delete Project
    /// </summary>
    /// <param name="projectUid">projectUid to delete</param>
    /// <remarks>Deletes existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/project/{projectUid}")]
    [HttpDelete]
    public async Task<ProjectV4DescriptorsSingleResult> DeleteProjectV4([FromUri]string projectUid)
    {
      log.LogInformation($"DeleteProjectV4. Project: {projectUid}");

      var project = new DeleteProjectEvent()
      {
        ProjectUID = Guid.Parse(projectUid),
        DeletePermanently = false,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      ProjectDataValidator.Validate(project, projectService);
      
      var messagePayload = JsonConvert.SerializeObject(new {DeleteProjectEvent = project});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
        });
      await projectService.StoreEvent(project).ConfigureAwait(false);

      log.LogInformation("DeleteProjectV4. Completed succesfully");
      return new ProjectV4DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV4Descriptor>(await GetProject(project.ProjectUID.ToString())
          .ConfigureAwait(false)));

    }

    #endregion projects


    #region subscriptions

    /// <summary>
    /// Gets available subscription for a customer
    /// </summary>
    /// <returns>List of available subscriptions</returns>
    [Route("api/v4/subscriptions")]
    [HttpGet]
    public async Task<SubscriptionsListResult> GetSubscriptionsV4()
    {
      log.LogInformation("GetSubscriptionsV4");
      var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      log.LogInformation("CustomerUID=" + customerUid + " and user=" + User);

      //return empty list if no subscriptions available
      return new SubscriptionsListResult()
      {
        SubscriptionDescriptors =
          (await GetFreeSubs(customerUid).ConfigureAwait(false)).Select(
            SubscriptionDescriptor.FromSubscription).ToImmutableList()
      };
    }

    #endregion subscriptions


    #region private

    /// <summary>
    /// Creates a project. Handles both old and new project boundary formats.
    /// </summary>
    /// <param name="project">The create project event</param>
    /// <param name="customerProject"></param>
    /// <returns></returns>
    protected async Task<CreateProjectEvent> CreateProject(CreateProjectEvent project, AssociateProjectCustomer customerProject)
    {
      log.LogDebug($"Creating the project {project.ProjectName}");

      var isCreated = await projectService.StoreEvent(project).ConfigureAwait(false);
      log.LogDebug($"Created the project in DB {isCreated}. legacyprojectID: {project.ProjectID}");

      if (project.ProjectID <= 0)
      {
        var existing = await projectService.GetProjectOnly(project.ProjectUID.ToString()).ConfigureAwait(false);
        if (existing != null && existing.LegacyProjectID > 0)
          project.ProjectID = existing.LegacyProjectID;
        else
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(42),
              contractExecutionStatesEnum.FirstNameWithOffset(42)));
        }
      }
      log.LogDebug($"Using Legacy projectId {project.ProjectID} for project {project.ProjectName}");

      // this is needed so that when ASNode (raptor client), which is called from CoordinateSystemPost, can retrieve the just written project+cp
      await projectService.StoreEvent(customerProject).ConfigureAwait(false);
      log.LogDebug($"Created CustomerProject {customerProject}");

      if (!string.IsNullOrEmpty(project.CoordinateSystemFileName))
      {
        var customHeaders = (Request.Headers.GetCustomHeaders());
        customHeaders.Add("X-VisionLink-ClearCache", "true");
        var coordinateSystemSettingsResult = await raptorProxy
          .CoordinateSystemPost(project.ProjectID, project.CoordinateSystemFileContent,
            project.CoordinateSystemFileName, customHeaders).ConfigureAwait(false);
        log.LogDebug($"Post of CS create to RaptorServices returned code: {0} Message {1}.",
          coordinateSystemSettingsResult?.Code ?? -1,
          coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null");
        if (coordinateSystemSettingsResult == null ||
            coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
        {
          await DeleteProjectPermanently(project.ProjectUID).ConfigureAwait(false);
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(41),
              string.Format(contractExecutionStatesEnum.FirstNameWithOffset(41),
                coordinateSystemSettingsResult?.Code ?? -1,
                coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null"
              )));
        }
      }
      return project; // legacyID may have been added
    }

    /// <summary>
    /// Determines if the project boundary overlaps any exising project for the customer in time and space.
    ///    not needed for v1 or 3 and they come via CGen which already does this overlap checking.
    /// </summary>    
    /// <param name="project">The create project event</param>
    /// <param name="databaseProjectBoundary">The project boundary in the new format (WKT)</param>
    /// <returns></returns>
    private async Task<bool> DoesProjectOverlap(CreateProjectEvent project, string databaseProjectBoundary)
    {
      var overlaps =
        await projectService.DoesPolygonOverlap(project.CustomerUID.ToString(), databaseProjectBoundary,
          project.ProjectStartDate, project.ProjectEndDate).ConfigureAwait(false);
      if (overlaps)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(43),
            contractExecutionStatesEnum.FirstNameWithOffset(43)));
      }
      log.LogDebug($"No overlapping projects for {project.ProjectName}");
      return overlaps;
    }


    /// <summary>
    /// Creates Kafka events.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="customerProject">The create projectCustomer event</param>
    /// <returns></returns>
    private async Task CreateKafkaEvents(CreateProjectEvent project, AssociateProjectCustomer customerProject)
    {
      log.LogDebug($"CreateProjectEvent on kafka queue {JsonConvert.SerializeObject(project)}");
      string wktBoundary = project.ProjectBoundary;
      
      // Convert to old format for Kafka for consistency on kakfa queue
      string kafkaBoundary = project.ProjectBoundary
        .Replace(ProjectBoundaryValidator.POLYGON_WKT, string.Empty)
        .Replace("))", string.Empty)
        .Replace(',', ';')
        .Replace(' ', ',');

      var messagePayloadProject = JsonConvert.SerializeObject(new { CreateProjectEvent = project });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayloadProject)
        });
      //Save boundary as WKT
      project.ProjectBoundary = wktBoundary;

      log.LogDebug($"AssociateCustomerProjectEvent on kafka queue {customerProject.ProjectUID} with Customer {customerProject.CustomerUID}");
      var messagePayloadCustomerProject = JsonConvert.SerializeObject(new {AssociateProjectCustomer = customerProject});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayloadCustomerProject)
        });
    }

    /// <summary>
    /// validate CordinateSystem if provided
    /// </summary>
    /// <param name=""></param>
    private async Task<bool> ValidateCoordSystem(IProjectEvent project)
    {
      // a Creating a landfill must have a CS, else optional
      //  if updating a landfill, or other then May have one. Note that a null one doesn't overwrite any existing.
      if (project is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent) project;
        if (projectEvent.ProjectType == ProjectType.LandFill
            && (string.IsNullOrEmpty(projectEvent.CoordinateSystemFileName)
                || projectEvent.CoordinateSystemFileContent == null)
        )
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(45),
              contractExecutionStatesEnum.FirstNameWithOffset(45)));
      }

      var csFileName = (project is CreateProjectEvent)
        ? ((CreateProjectEvent) project).CoordinateSystemFileName
        : ((UpdateProjectEvent) project).CoordinateSystemFileName;
      var csFileContent = (project is CreateProjectEvent)
        ? ((CreateProjectEvent) project).CoordinateSystemFileContent
        : ((UpdateProjectEvent) project).CoordinateSystemFileContent;
      if (!string.IsNullOrEmpty(csFileName) || csFileContent != null)
      {
        ProjectDataValidator.ValidateFileName(csFileName);
        var coordinateSystemSettingsResult = await raptorProxy
          .CoordinateSystemValidate(csFileContent, csFileName, Request.Headers.GetCustomHeaders())
          .ConfigureAwait(false);
        if (coordinateSystemSettingsResult == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(46),
              contractExecutionStatesEnum.FirstNameWithOffset(46)));
        }
        if (coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(47),
              string.Format(contractExecutionStatesEnum.FirstNameWithOffset(47), coordinateSystemSettingsResult.Code,
                coordinateSystemSettingsResult.Message)));
        }
      }
      return true;
    }

    /// <summary>
    /// Used internally, if a step fails, after a project has been CREATED, 
    ///    then delete it permanently i.e. don't just set IsDeleted.
    /// It should not be necessary to delete any other database items?
    ///     CustomerProject? AssociateSubscription?
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    private async Task DeleteProjectPermanently(Guid projectUid)
    {
      var deleteProjectEvent = new DeleteProjectEvent()
      {
        ProjectUID = projectUid,
        DeletePermanently = true,
        ActionUTC = DateTime.UtcNow
      };
      var isDeleted = await projectService.StoreEvent(deleteProjectEvent).ConfigureAwait(false);
    }

    #endregion private

  }
}


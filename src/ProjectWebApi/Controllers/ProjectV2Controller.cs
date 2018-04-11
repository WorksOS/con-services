using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project controller v2
  /// This is used by BusinessCenter. 
  ///     The signature must be retained.
  ///     BC is now compatible with jwt/TID etc.   
  /// </summary>
  public class ProjectV2Controller : ProjectBaseController
  {
    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    private ProjectRequestHelper projectRequestHelper;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="projectRepo"></param>
    /// <param name="subscriptionsRepo"></param>
    /// <param name="store"></param>
    /// <param name="subscriptionProxy"></param>
    /// <param name="geofenceProxy"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="fileRepo"></param>
    /// <param name="logger"></param>
    /// <param name="serviceExceptionHandler">The ServiceException handler.</param>
    public ProjectV2Controller(IKafka producer, IProjectRepository projectRepo,
      ISubscriptionRepository subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subscriptionProxy,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, IFileRepository fileRepo,
      ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler)
      : base(producer, projectRepo, subscriptionsRepo, store, subscriptionProxy, geofenceProxy, raptorProxy, fileRepo,
        logger, serviceExceptionHandler, logger.CreateLogger<ProjectV2Controller>())
    {
      this.logger = logger;
      this.projectRequestHelper = new ProjectRequestHelper(
        log, configStore, serviceExceptionHandler,
        customerUid, userId, customHeaders,
        producer,
        geofenceProxy, raptorProxy, subscriptionProxy,
        projectRepo, subscriptionRepo, fileRepo);
    }

    #region projects

    ///// <summary>
    ///// Gets a project for a customer. 
    ///// </summary>
    ///// <returns>A project data</returns>
    // not needed for BCC?
    //[Route("api/v2/projects/{id}")]
    //[HttpGet]
    //public async Task<ProjectV2DescriptorResult> GetProjectV2(long id)
    //{
    //  logger.LogInformation("GetProjectV2");
    //  var project = await GetProject(id).ConfigureAwait(false);
    //  return AutoMapperUtility.Automapper.Map<ProjectV2DescriptorResult>(project);
    //}

    // POST: api/v2/projects
    /// <summary>
    /// Update Project
    /// </summary>
    /// <param name="projectRequest">UpdateProjectRequest model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v2/projects")]
    [HttpPost]
    public async Task<ContractExecutionResult> CreateProjectV2([FromBody] CreateProjectV2Request projectRequest)
    {
      if (projectRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 81);
      }

      log.LogInformation("CreateProjectV2. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));

      var createProjectEvent = MapV2Models.MapCreateProjectV2RequestToEvent(projectRequest, customerUid);

      ProjectDataValidator.Validate(createProjectEvent, projectRepo);
      projectRequest.CoordinateSystem = ProjectDataValidator.ValidateBusinessCentreFile(projectRequest.CoordinateSystem);
      // get CoordinateSystem file content from TCC
      createProjectEvent.CoordinateSystemFileContent = await projectRequestHelper.GetCoordinateSystemContent(projectRequest.CoordinateSystem).ConfigureAwait(false);

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreateProjectExecutor>(logger, configStore, serviceExceptionHandler,
            customerUid, userId, null, customHeaders,
            producer, kafkaTopicName,
            geofenceProxy, raptorProxy, SubscriptionProxy,
            projectRepo, subscriptionRepo, fileRepo)
          .ProcessAsync(createProjectEvent)
      );

      log.LogDebug("CreateProjectV2. completed succesfully");
      return new ContractExecutionResult((int)(HttpStatusCode.Created), string.Format($"{createProjectEvent.ProjectID}"));
    }


    //// DELETE: api/v2/projects/
    ///// <summary>
    ///// Delete Project
    ///// </summary>
    ///// <param name="id">legacyProjectId to delete</param>
    ///// <remarks>Deletes existing project</remarks>
    ///// <response code="200">Ok</response>
    ///// <response code="400">Bad request</response>
    //  not needed for BCC?
    //[Route("api/v2/projects/{id}")]
    //[HttpDelete]
    //public async Task<ContractExecutionResult> DeleteProjectV2([FromUri] long id)
    //{
    //  LogCustomerDetails("DeleteProjectV2", id.ToString());

    //  var project = await GetProject(id).ConfigureAwait(false);
    //  if (project == null)
    //  { 
    //    // return new bool(); // todo
    //    serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, /* 39 todo */ 0);
    //  }

    //  var deleteProjectEvent = new DeleteProjectEvent
    //  {
    //    ProjectUID = Guid.Parse(project.ProjectUID),
    //    DeletePermanently = false,
    //    ActionUTC = DateTime.UtcNow,
    //    ReceivedUTC = DateTime.UtcNow
    //  };
    //  // no need to validate as already checked that it exists
    //  // ProjectDataValidator.Validate(projectToDelete, projectRepo);

    //  var messagePayload = JsonConvert.SerializeObject(new { DeleteProjectEvent = project });
    //  var isDeleted = await projectRepo.StoreEvent(deleteProjectEvent).ConfigureAwait(false);
    //  if (isDeleted == 0)
    //    serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 66);

    //  producer.Send(kafkaTopicName,
    //    new List<KeyValuePair<string, string>>
    //    {
    //      new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
    //    });

    //  logger.LogInformation("DeleteProjectV2. Completed succesfully");
    //  return new ContractExecutionResult();
    //}

    #endregion projects
  }
}
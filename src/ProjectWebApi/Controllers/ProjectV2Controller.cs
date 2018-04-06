using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
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
    /// 
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="projectRepo"></param>
    /// <param name="subscriptionsRepo"></param>
    /// <param name="store"></param>
    /// <param name="subsProxy"></param>
    /// <param name="geofenceProxy"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="logger"></param>
    /// <param name="serviceExceptionHandler">The ServiceException handler.</param>
    public ProjectV2Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler)
      : base(producer, projectRepo, subscriptionsRepo, store, subsProxy, geofenceProxy, raptorProxy, logger,
        serviceExceptionHandler, logger.CreateLogger<ProjectV2Controller>())
    { }

    #region projects

    /// <summary>
    /// Gets a list of projects for a customer. 
    /// BC aleady uses NGen ProjectSvc for this no not required.
    /// </summary>
    /// <returns>A list of projects</returns>
    //[Route("api/v2/project/all")]
   

    /// <summary>
    /// Gets a project for a customer. 
    /// </summary>
    /// <returns>A project data</returns>
    [Route("api/v2/projects/{id}")]
    [HttpGet]
    public async Task<ProjectV2DescriptorsSingleResult> GetProjectV2(long id)
    {
      log.LogInformation("GetProjectV2");
      var project = await GetProject(id).ConfigureAwait(false);
      return new ProjectV2DescriptorsSingleResult(AutoMapperUtility.Automapper.Map<ProjectV2Descriptor>(project));
    }

 
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
    public async Task<ProjectV2DescriptorsSingleResult> CreateProjectV2([FromBody] CreateProjectV2Request projectRequest)
    {
      if (projectRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, /* 39 todo */ 0);
      }

      log.LogInformation("CreateProjectV2. projectRequest: {0}", JsonConvert.SerializeObject(projectRequest));

     // todo how to get legacyCustomerId? could it be missing i.e. 0?
      var project = MapV2Models.MapCreateProjectV2RequestToEvent(projectRequest, customerUid, legacyCustomerId:0);
      
      // todo are validation rules the same?
      ProjectDataValidator.Validate(project, projectRepo);
      
      await ValidateCoordSystemInRaptor(project).ConfigureAwait(false);

      log.LogDebug($"Testing if there are overlapping projects for project {project.ProjectName}");
      await DoesProjectOverlap(project, project.ProjectBoundary);

      AssociateProjectCustomer customerProject = new AssociateProjectCustomer
      {
        CustomerUID = project.CustomerUID,
        LegacyCustomerID = project.CustomerID,
        ProjectUID = project.ProjectUID,
        RelationType = RelationType.Owner,
        ActionUTC = project.ActionUTC,
        ReceivedUTC = project.ReceivedUTC
      };
      ProjectDataValidator.Validate(customerProject, projectRepo);

      /*** now making changes, potentially needing rollback ***/
      project = await CreateProjectInDb(project, customerProject).ConfigureAwait(false);

      /*
       *  todo need to get CoordinateSystemFileContent. in cGen this is how it does it
       *     add to tcc then read content as byte[] for Raptor???
      if (projectID != -1) // if successful update coordinate system in Raptor
      {
        if (data == null)
          data = TCCHelper.LoginToBusinessCenterAsVL(session, projectID);

        CoordinateSettingsBase coordSystem = new CoordinateSettingsBase()
        {
          filespaceID = coordinateSystem.filespaceID,
          sourcePath = coordinateSystem.path,
          name = coordinateSystem.name
        };

        if (TCCHelper.AddUpdateCoordinateSettings(session, coordSystem, projectID, data, out errorToken))
        {
          if (coordinateSystem != null)
            ProjectMonitoringHelper.CleanUpTempFiles(sessionID);
        }
        else
          return -1;
      }
       */
      await CreateCoordSystemInRaptor(project.ProjectUID, project.ProjectID, project.CoordinateSystemFileName, project.CoordinateSystemFileContent, true).ConfigureAwait(false);
      await AssociateProjectSubscriptionInSubscriptionService(project).ConfigureAwait(false);
      await CreateGeofenceInGeofenceService(project).ConfigureAwait(false);       // todo Associate ProjectGeofence

      // doing this as late as possible in case something fails. We can't cleanup kafka que.
      CreateKafkaEvents(project, customerProject);

      log.LogDebug("CreateProjectV4. completed succesfully");
      return new ProjectV2DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV2Descriptor>(await GetProject(project.ProjectUID.ToString())
          .ConfigureAwait(false)));
    }


    // DELETE: api/v2/projects/
    /// <summary>
    /// Delete Project
    /// </summary>
    /// <param name="id">legacyProjectId to delete</param>
    /// <remarks>Deletes existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v2/projects/{id}")]
    [HttpDelete]
    public async /* todo Task<bool>*/ Task<ContractExecutionResult> DeleteProjectV2([FromUri] long id)
    {
      LogCustomerDetails("DeleteProjectV2", id.ToString());

      var project = await GetProject(id).ConfigureAwait(false);
      if (project == null)
      { 
        // return new bool(); // todo
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, /* 39 todo */ 0);
      }

      var projectToDelete = new DeleteProjectEvent
      {
        ProjectUID = Guid.Parse(project.ProjectUID),
        DeletePermanently = false,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      // no need to validate as already checked that it exists
      // ProjectDataValidator.Validate(projectToDelete, projectRepo);

      var messagePayload = JsonConvert.SerializeObject(new { DeleteProjectEvent = project });
      var isDeleted = await projectRepo.StoreEvent(projectToDelete).ConfigureAwait(false);
      if (isDeleted == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 66);

      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
        });

      log.LogInformation("DeleteProjectV4. Completed succesfully");
      return new ContractExecutionResult();
    }

    #endregion projects
  }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which creates a project - appropriate for v2 and v4 controllers
  /// </summary>
  public class CreateProjectExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Save for potential rollback
    /// </summary>
    protected string subscriptionUidAssigned;

   
    /// <summary>
    /// Processes the CreateProjectEvent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ContractExecutionResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      CreateProjectEvent createProjectEvent = item as CreateProjectEvent;
      if (createProjectEvent == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);
      }

      ProjectRequestHelper.ValidateProjectBoundary(createProjectEvent.ProjectBoundary, serviceExceptionHandler);

      ProjectRequestHelper.ValidateCoordSystemFile(null, createProjectEvent, serviceExceptionHandler);

      await ProjectRequestHelper.ValidateCoordSystemInRaptor(createProjectEvent,
        serviceExceptionHandler, customHeaders, raptorProxy).ConfigureAwait(false);

      log.LogDebug($"Testing if there are overlapping projects for project {createProjectEvent.ProjectName}");
      await ProjectRequestHelper.DoesProjectOverlap(createProjectEvent.CustomerUID.ToString(),
        createProjectEvent.ProjectUID.ToString(),
        createProjectEvent.ProjectStartDate, createProjectEvent.ProjectEndDate, createProjectEvent.ProjectBoundary,
        log, serviceExceptionHandler, projectRepo);

      AssociateProjectCustomer customerProject = new AssociateProjectCustomer
      {
        CustomerUID = createProjectEvent.CustomerUID,
        LegacyCustomerID = createProjectEvent.CustomerID,
        ProjectUID = createProjectEvent.ProjectUID,
        RelationType = RelationType.Owner,
        ActionUTC = createProjectEvent.ActionUTC,
        ReceivedUTC = createProjectEvent.ReceivedUTC
      };
      ProjectDataValidator.Validate(customerProject, projectRepo, serviceExceptionHandler);
      await ProjectDataValidator.ValidateFreeSub(customerUid, createProjectEvent.ProjectType,
        log, serviceExceptionHandler, subscriptionRepo);
      log.LogDebug($"CreateProject: passed validation {createProjectEvent.ProjectUID}");


      // now making changes, potentially needing rollback 
      //  order changes to minimise rollback
      //    if CreateProjectInDb fails then nothing is done
      //    if CreateCoordSystem fails then project is deleted
      //    if AssociateProjectSubscription fails ditto
      createProjectEvent = await CreateProjectInDb(createProjectEvent, customerProject).ConfigureAwait(false);
      await ProjectRequestHelper.CreateCoordSystemInRaptorAndTcc(
        createProjectEvent.ProjectUID, createProjectEvent.ProjectID, createProjectEvent.CoordinateSystemFileName, 
        createProjectEvent.CoordinateSystemFileContent, true, log, serviceExceptionHandler, customerUid, customHeaders,
        projectRepo, raptorProxy, configStore, fileRepo, dataOceanClient, authn).ConfigureAwait(false);
      log.LogDebug($"CreateProject: Created project {createProjectEvent.ProjectUID}");

      subscriptionUidAssigned = await ProjectRequestHelper.AssociateProjectSubscriptionInSubscriptionService(createProjectEvent.ProjectUID.ToString(), createProjectEvent.ProjectType, customerUid,
        log, serviceExceptionHandler, customHeaders, subscriptionProxy,subscriptionRepo, projectRepo, true).ConfigureAwait(false);
      log.LogDebug($"CreateProject: Was projectSubscription Associated? subscriptionUidAssigned: {subscriptionUidAssigned}");
    
      // doing this as late as possible in case something fails. We can't cleanup kafka que.
      CreateKafkaEvents(createProjectEvent, customerProject);

      log.LogDebug("CreateProject. completed succesfully");
      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }


    /// <summary>
    /// Creates a project. Handles both old and new project boundary formats.
    /// </summary>
    /// <param name="project">The create project event</param>
    /// <param name="customerProject"></param>
    /// <returns></returns>
    private async Task<CreateProjectEvent> CreateProjectInDb(CreateProjectEvent project,
      AssociateProjectCustomer customerProject)
    {
      log.LogDebug(
        $"Creating the project in the DB {JsonConvert.SerializeObject(project)} and customerProject {JsonConvert.SerializeObject(customerProject)}");

      var isCreated = 0;
      try
      {
        isCreated = await projectRepo.StoreEvent(project).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 61, "projectRepo.storeCreateProject", e.Message);
      }

      if (isCreated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 61);

      log.LogDebug(
        $"Created the project in DB. IsCreated: {isCreated}. projectUid: {project.ProjectUID} legacyprojectID: {project.ProjectID}");

      if (project.ProjectID <= 0)
      {
        Repositories.DBModels.Project existing = null;
        try
        {
          existing = await projectRepo.GetProjectOnly(project.ProjectUID.ToString()).ConfigureAwait(false);
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 42, "projectRepo.GetProjectOnly", e.Message);
        }
        if (existing != null && existing.LegacyProjectID > 0)
          project.ProjectID = existing.LegacyProjectID;
        else
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 42);
        }
      }

      log.LogDebug($"Using Legacy projectId {project.ProjectID} for project {project.ProjectName}");

      // this is needed so that when ASNode (raptor client), which is called from CoordinateSystemPost, can retrieve the just written project+cp
      try
      {
        isCreated = await projectRepo.StoreEvent(customerProject).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 63, "projectRepo.StoreCustomerProject", e.Message);
      }
      if (isCreated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 63);

      log.LogDebug($"Created CustomerProject in DB {JsonConvert.SerializeObject(customerProject)}");
      return project; // legacyID may have been added
    }


    /// <summary>
    /// Creates Kafka events.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="customerProject">The create projectCustomer event</param>
    /// <returns></returns>
    protected void CreateKafkaEvents(CreateProjectEvent project, AssociateProjectCustomer customerProject)
    {
      log.LogDebug($"CreateProjectEvent on kafka queue {JsonConvert.SerializeObject(project)}");
      string wktBoundary = project.ProjectBoundary;

      // Convert to old format for Kafka for consistency on kakfa queue
      string kafkaBoundary = project.ProjectBoundary
        .Replace(GeofenceValidation.POLYGON_WKT, string.Empty)
        .Replace("))", string.Empty)
        .Replace(',', ';')
        .Replace(' ', ',');

      var messagePayloadProject = JsonConvert.SerializeObject(new { CreateProjectEvent = project });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayloadProject)
        });
      //Save boundary as WKT
      project.ProjectBoundary = wktBoundary;

      log.LogDebug(
        $"AssociateCustomerProjectEvent on kafka queue {customerProject.ProjectUID} with Customer {customerProject.CustomerUID}");
      var messagePayloadCustomerProject = JsonConvert.SerializeObject(new { AssociateProjectCustomer = customerProject });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayloadCustomerProject)
        });
    }

  }
}
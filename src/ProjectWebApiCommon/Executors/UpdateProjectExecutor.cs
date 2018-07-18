using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which updates a updateProjectEvent - appropriate for v4 controller
  /// </summary>
  public class UpdateProjectExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Save for potential rollback
    /// </summary>
    protected string subscriptionUidAssigned = null;

    /// <summary>
    /// Processes the UpdateProjectEvent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ContractExecutionResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      UpdateProjectEvent updateProjectEvent = item as UpdateProjectEvent;
      if (updateProjectEvent == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);
      }

      var existing = await projectRepo.GetProject(updateProjectEvent.ProjectUID.ToString());
      if (existing == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 7);
      }

      ProjectRequestHelper.ValidateGeofence(updateProjectEvent.ProjectBoundary, serviceExceptionHandler);

      // if updating  type from Standard to Landfill, or creating Landfill, then must have a CoordSystem
      ProjectRequestHelper.ValidateCoordSystemFile(existing, updateProjectEvent, serviceExceptionHandler);

      await ProjectRequestHelper.ValidateCoordSystemInRaptor(updateProjectEvent, serviceExceptionHandler, customHeaders, raptorProxy).ConfigureAwait(false);

      if (!string.IsNullOrEmpty(updateProjectEvent.ProjectBoundary) && String.Compare(existing.GeometryWKT,
            updateProjectEvent.ProjectBoundary, StringComparison.OrdinalIgnoreCase) != 0)
      {
        await ProjectRequestHelper.DoesProjectOverlap(existing.CustomerUID, updateProjectEvent.ProjectUID.ToString(),
          existing.StartDate, updateProjectEvent.ProjectEndDate, updateProjectEvent.ProjectBoundary,
          log, serviceExceptionHandler, projectRepo);
      }

      if (existing != null && existing.ProjectType != updateProjectEvent.ProjectType)
      {
        if (existing.ProjectType != ProjectType.Standard || updateProjectEvent.ProjectType == ProjectType.Standard)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 85);
        }

        await ProjectDataValidator.ValidateFreeSub(customerUid, updateProjectEvent.ProjectType, log, serviceExceptionHandler, subscriptionRepo);
      }

      log.LogDebug($"UpdateProject: passed validation {updateProjectEvent.ProjectUID}");

      /*** now making changes, potentially needing rollback ***/
      //  order changes to minimise rollback
      //    if CreateCoordSystemInRaptorAndTcc fails then nothing is done
      //    if AssociateProjectSubscription fails then nothing is done
      //    if UpdateProjectEvent fails then ProjectSubscription is Dissassociated
      //    if UpdateGeofenceInGeofenceService fails then any Project changes and ProjectSubscription is Dissassociated 
      if (!string.IsNullOrEmpty(updateProjectEvent.CoordinateSystemFileName))
      {
        // don't bother rolling this back
        await ProjectRequestHelper.CreateCoordSystemInRaptorAndTcc(updateProjectEvent.ProjectUID,
          existing.LegacyProjectID,
          updateProjectEvent.CoordinateSystemFileName, updateProjectEvent.CoordinateSystemFileContent, false,
          log, serviceExceptionHandler, customerUid, customHeaders,
          projectRepo, raptorProxy, configStore, fileRepo).ConfigureAwait(false);
        log.LogDebug($"UpdateProject: CreateCoordSystemInRaptorAndTcc succeeded");
      }

      if (existing != null && existing.ProjectType == ProjectType.Standard &&
          (updateProjectEvent.ProjectType == ProjectType.LandFill ||
           updateProjectEvent.ProjectType == ProjectType.ProjectMonitoring))
      {
        subscriptionUidAssigned = await ProjectRequestHelper.AssociateProjectSubscriptionInSubscriptionService(
            updateProjectEvent.ProjectUID.ToString(), updateProjectEvent.ProjectType, customerUid,
            log, serviceExceptionHandler, customHeaders, subscriptionProxy, subscriptionRepo, projectRepo, false)
          .ConfigureAwait(false);
      }

      log.LogDebug($"UpdateProject: subscriptionUidAssigned? {subscriptionUidAssigned}. ExistingProjectType: {existing.ProjectType} updatedProjectType: {updateProjectEvent.ProjectType}");

      var isUpdated = 0;
      try
      {
        isUpdated = await projectRepo.StoreEvent(updateProjectEvent).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 62, "projectRepo.storeUpdateProject", e.Message);
      }

      if (isUpdated == 0)
      {
        await RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 62, string.Empty);
      }

      log.LogDebug($"UpdateProject: Project updated successfully");


      if (!string.IsNullOrEmpty(updateProjectEvent.ProjectBoundary) && String.Compare(existing.GeometryWKT,
            updateProjectEvent.ProjectBoundary, StringComparison.OrdinalIgnoreCase) != 0)
      {
        await UpdateGeofenceInGeofenceService(updateProjectEvent, existing);
      }

      // doing this as late as possible in case something fails. We can't cleanup kafka que.
      CreateKafkaEvents(updateProjectEvent);

      log.LogDebug("UpdateProjectV4. completed succesfully");
      return new ContractExecutionResult();
    }

    /// <summary>
    /// Creates Kafka events.
    /// </summary>
    /// <param name="updateProjectEvent"></param>
    /// <returns></returns>
    protected void CreateKafkaEvents(UpdateProjectEvent updateProjectEvent)
    {
      var messagePayload = JsonConvert.SerializeObject(new {UpdateProjectEvent = updateProjectEvent});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(updateProjectEvent.ProjectUID.ToString(), messagePayload)
        });
    }

    /// <summary>
    /// Updates the projectType geofence
    ///     If something fails, need to rollback
    ///        changes to updateProjectEvent, projectSub
    /// </summary>
    /// <param name="updateProjectEvent">The updateProjectEvent.</param>
    /// <param name="existing"></param>
    /// <returns></returns>
    protected async Task UpdateGeofenceInGeofenceService(UpdateProjectEvent updateProjectEvent, Repositories.DBModels.Project existing)
    {
      log.LogDebug($"UpdateProjectGeofence: Updating the Project geofence: {updateProjectEvent.ProjectUID.ToString()}");

      List<ProjectGeofence> projectGeofences = null;
      ProjectGeofence projectGeofence = null;
      try
      {
        projectGeofences = (await projectRepo.GetAssociatedGeofences(updateProjectEvent.ProjectUID.ToString())
          .ConfigureAwait(false)).ToList();
      }
      catch (Exception e)
      {
        await RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 101, e.Message, existing);
      }

      if (projectGeofences != null && projectGeofences.Any())
      {
        projectGeofence = projectGeofences.FirstOrDefault(p => p.GeofenceType == GeofenceType.Project);

        if (projectGeofence == null)
        {
          projectGeofence = await MissMatchedGeofence(projectGeofences, existing).ConfigureAwait(false);
        }
      }

      if (projectGeofence == null)
      {
        await CreateProjectGeofence(updateProjectEvent, existing).ConfigureAwait(false);
        return;
      }
      
      log.LogDebug(
        $"UpdateProjectGeofence: Got the ProjectGeofence association: {JsonConvert.SerializeObject(projectGeofence)}");


      GeofenceData geofence = null;
      try
      {
        geofence = await geofenceProxy.GetGeofenceForCustomer(customerUid, projectGeofence.GeofenceUID, customHeaders)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        await RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 98, e.Message, existing);
      }

      if (geofence == null)
      {
        await RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 97, string.Empty, existing);
      }

      log.LogDebug($"UpdateProject: Got the geofence: {JsonConvert.SerializeObject(geofence)}");


      Guid geofenceUidUpdated = Guid.Empty;
      try
      {
        var area = GeofenceValidation.CalculateAreaSqMeters(updateProjectEvent.ProjectBoundary);
        log.LogDebug($"UpdateProject: Going to update Geofence. Area: {area}, customerUid: {customerUid}");
        geofenceUidUpdated = await geofenceProxy.UpdateGeofence(geofence.GeofenceUID, Guid.Parse(customerUid),
          geofence.GeofenceName,
          geofence.Description, geofence.GeofenceType,
          updateProjectEvent.ProjectBoundary,
          geofence.FillColor, geofence.IsTransparent, geofence.UserUID, area, customHeaders).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        await RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 100, e.Message, existing);
      }

      if (geofenceUidUpdated == Guid.Empty)
      {
        await RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 99, String.Empty, existing);
      }

      log.LogDebug($"UpdateProject: geoFence has been updated: {geofenceUidUpdated}");

    }

    private async Task<ProjectGeofence> MissMatchedGeofence(List<ProjectGeofence> projectGeofences, Repositories.DBModels.Project existing)
    {
      // is this project pointing to the wrong geofence due to geofenceSvc issue
      //  where the GeofenceService doesn't always use the GeofenceUID passed to it.
      //  This caused the geofence to be created with a different GeofenceUID,
      //      but the ProjectGeofence with the expected GeofenceUID.
      //  Need to unassociate with the one which doesn't exist, and associate with the existing one.
      // mmmm not nice at all to fix data issues in main code, however due to having to keep kafka que in sync...
      //      can't just create a new Geofence, as the GeofenceSvc will not allow one with the same name.
      //      If this fails, however (e.g. if user has changed project name), then the next step (CreateGeofence) may indeed create a new one.

      //  i.e. is ProjectGeofence pointing to a missing Geofence?
      ProjectGeofence newlyMatchedProjectGeofence = null;
      var psIsMissingGeofence = projectGeofences.FirstOrDefault(p => p.GeofenceType == null);
      if (psIsMissingGeofence != null)
      {
        log.LogInformation($"MissMatchedGeofence: Got a ProjectGeofence which is missing the Geofence {JsonConvert.SerializeObject(psIsMissingGeofence)}");
        var customerGeofences = (await projectRepo.GetCustomerGeofences(customerUid).ConfigureAwait(false)).ToList();
        
        // if geofence doesn't exist, then we're on to it
        if (customerGeofences.Any(g => g.GeofenceUID == psIsMissingGeofence.GeofenceUID) == false)
        {
          var gotFreeProjectGeofenceOfSameName = customerGeofences.FirstOrDefault(g =>
              g.GeofenceType == GeofenceType.Project && g.ProjectUID == null && g.Name == existing.Name);

          log.LogDebug($"MissMatchedGeofence: Geofence indeed does not exist. Do we have an unattached, appropriate Geofence? {gotFreeProjectGeofenceOfSameName}");
          if (gotFreeProjectGeofenceOfSameName != null)
          {
            var dissociateProjectGeofence = new DissociateProjectGeofence()
            {
              ProjectUID = Guid.Parse(psIsMissingGeofence.ProjectUID),
              GeofenceUID = Guid.Parse(psIsMissingGeofence.GeofenceUID),
              ActionUTC = DateTime.UtcNow
            };

            await ProjectRequestHelper.DissociateProjectGeofence(dissociateProjectGeofence, projectRepo, log,
              serviceExceptionHandler, producer, kafkaTopicName);
            log.LogDebug($"MissMatchedGeofence: DissociatedProject from missing Geofence: {dissociateProjectGeofence}");

            var associateProjectGeofence = new AssociateProjectGeofence()
            {
              ProjectUID = Guid.Parse(psIsMissingGeofence.ProjectUID),
              GeofenceUID = Guid.Parse(gotFreeProjectGeofenceOfSameName.GeofenceUID),
              ActionUTC = DateTime.UtcNow
            };

            await ProjectRequestHelper
              .AssociateProjectGeofence(associateProjectGeofence, projectRepo,
                log, serviceExceptionHandler,
                producer, kafkaTopicName).ConfigureAwait(false);
            log.LogInformation($"MissMatchedGeofence: re-associated Project with unattached, appropriate Geofence: {associateProjectGeofence}");

            newlyMatchedProjectGeofence = new ProjectGeofence()
            {
              GeofenceType = GeofenceType.Project,
              ProjectUID = psIsMissingGeofence.ProjectUID,
              GeofenceUID = gotFreeProjectGeofenceOfSameName.GeofenceUID
            };
          }
        }
      }

      return newlyMatchedProjectGeofence;
    }

    private async Task CreateProjectGeofence(UpdateProjectEvent updateProjectEvent, Repositories.DBModels.Project existing)
    {
      // this patches the fact that various prior ProjectSvc CreateProject endpoints including TBC intentionally)
      // didn't (or couldn't - TBC) add Geofence and/or ProjectGeofence. Add them now.

      log.LogDebug($"UpdateProject.CreateProjectGeofence.");

      // if ExistsExpression in  projectGeofences, one with genericType, that's probably a null one


      // Create Geofence
      var geofenceUidCreated = Guid.Empty;
      try
      {
        geofenceUidCreated = await ProjectRequestHelper.CreateGeofenceInGeofenceService(
          updateProjectEvent.ProjectUID.ToString(), updateProjectEvent.ProjectName, updateProjectEvent.ProjectBoundary,
          customerUid, userId,
          httpContextAccessor, log, serviceExceptionHandler,
          customHeaders, geofenceProxy).ConfigureAwait(false);
        log.LogDebug($"UpdateProject: Was geofence created by GeofenceSvc? geofenceUid: {geofenceUidCreated}");
      }
      catch (Exception e)
      {
        await RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 59, e.Message, existing);
      }

      if (geofenceUidCreated == Guid.Empty)
      {
        await RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 59, string.Empty, existing);
      }

      AssociateProjectGeofence associateProjectGeofence = new AssociateProjectGeofence()
      {
        ProjectUID = updateProjectEvent.ProjectUID,
        GeofenceUID = geofenceUidCreated,
        ActionUTC = DateTime.UtcNow
      };

      await ProjectRequestHelper
        .AssociateProjectGeofence(associateProjectGeofence, projectRepo,
          log, serviceExceptionHandler,
          producer, kafkaTopicName).ConfigureAwait(false);
      log.LogDebug("UpdateProject. associateProjectGeofence: {associateProjectGeofence}");
    }


    private async Task RollbackAndThrow(UpdateProjectEvent updateProjectEvent, HttpStatusCode httpStatusCode, int errorCode, string exceptionMessage, Repositories.DBModels.Project existing = null)
    {
      log.LogDebug($"Rolling back the Project Update for updateProjectEvent: {updateProjectEvent.ProjectUID.ToString()} subscriptionUidAssigned: {subscriptionUidAssigned}");

      await ProjectRequestHelper.DissociateProjectSubscription(updateProjectEvent.ProjectUID, subscriptionUidAssigned, log, customHeaders, subscriptionProxy).ConfigureAwait(false);

      if (existing != null)
      {
        // rollback changes to Project
        var rollbackProjectEvent = AutoMapperUtility.Automapper.Map<UpdateProjectEvent>(updateProjectEvent);
        rollbackProjectEvent.ProjectEndDate = existing.EndDate;
        rollbackProjectEvent.ProjectTimezone = existing.ProjectTimeZone;
        rollbackProjectEvent.ProjectName = existing.Name;
        rollbackProjectEvent.Description = existing.Description;
        rollbackProjectEvent.ProjectType = existing.ProjectType;
        rollbackProjectEvent.ProjectBoundary = existing.GeometryWKT;
        rollbackProjectEvent.CoordinateSystemFileName = existing.CoordinateSystemFileName;
        rollbackProjectEvent.ActionUTC = DateTime.UtcNow;

        var isUpdated = await projectRepo.StoreEvent(rollbackProjectEvent).ConfigureAwait(false);
        log.LogDebug($"UpdateProject: Rolled back Project changes. Updated count (should be 1): {isUpdated}");
      }

      serviceExceptionHandler.ThrowServiceException(httpStatusCode, errorCode, exceptionMessage);
    }
    
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
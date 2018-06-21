using System;
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
      log.LogDebug($"UpdateProject: subscriptionUidAssigned: {subscriptionUidAssigned} existing.ProjectType {existing.ProjectType} updateProjectEvent.ProjectType: {updateProjectEvent.ProjectType}");

      var isUpdated = await projectRepo.StoreEvent(updateProjectEvent).ConfigureAwait(false);
      if (isUpdated == 0)
      {
        RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 62, string.Empty);
      }
      log.LogDebug($"UpdateProject: Project updated successfully");


      if (!string.IsNullOrEmpty(updateProjectEvent.ProjectBoundary) && String.Compare(existing.GeometryWKT,
            updateProjectEvent.ProjectBoundary, StringComparison.OrdinalIgnoreCase) != 0)
      {
        await UpdateGeofenceInGeofenceService(updateProjectEvent, existing);
      }

      log.LogDebug("UpdateProjectV4. completed succesfully");
      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
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
      log.LogDebug($"UpdateProject: Updating the Project geofence: {updateProjectEvent.ProjectUID.ToString()}");

      ProjectGeofence projectGeofence = null;
      try
      {
        projectGeofence =
          (await projectRepo.GetAssociatedGeofences(updateProjectEvent.ProjectUID.ToString()).ConfigureAwait(false))
          .FirstOrDefault(
            p => p.GeofenceType == GeofenceType.Project);
      }
      catch (Exception e)
      {
        RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 101, e.Message, existing);
      }
      if (projectGeofence == null)
      {
        RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 96, string.Empty, existing);
      }
      log.LogDebug($"UpdateProject: Got the ProjectGeofence association: {JsonConvert.SerializeObject(projectGeofence)}");


      GeofenceData geofence = null;
      try
      {
        geofence = await geofenceProxy.GetGeofenceForCustomer(customerUid, projectGeofence.GeofenceUID, customHeaders)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 98, e.Message, existing);
      }
      if (geofence == null)
      {
        RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 97, string.Empty, existing);
      }
      log.LogDebug($"UpdateProject: Got the geofence: {JsonConvert.SerializeObject(geofence)}");


      Guid geofenceUidUpdated = Guid.Empty;
      try
      {
        var area = GeofenceValidation.CalculateAreaSqMeters(updateProjectEvent.ProjectBoundary);
        geofenceUidUpdated = await geofenceProxy.UpdateGeofence(geofence.GeofenceUID, Guid.Parse(customerUid),
          geofence.GeofenceName,
          geofence.Description, geofence.GeofenceType,
          updateProjectEvent.ProjectBoundary,
          geofence.FillColor, geofence.IsTransparent, geofence.UserUID, area, customHeaders).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 100, e.Message, existing);
      }
      if (geofenceUidUpdated == Guid.Empty)
      {
        RollbackAndThrow(updateProjectEvent, HttpStatusCode.InternalServerError, 99, String.Empty, existing);
      }
      log.LogDebug($"UpdateProject: geoFence has been updated: {geofenceUidUpdated}");

    }

    private async void RollbackAndThrow(UpdateProjectEvent updateProjectEvent, HttpStatusCode httpStatusCode, int errorCode, string exceptionMessage, Repositories.DBModels.Project existing = null)
    {
      log.LogDebug($"Rolling back the Project Update for updateProjectEvent: {updateProjectEvent.ProjectUID.ToString()} subscriptionUidAssigned: {subscriptionUidAssigned}");

      if (!string.IsNullOrEmpty(subscriptionUidAssigned))
      {
        await ProjectRequestHelper.DissociateProjectSubscription(updateProjectEvent.ProjectUID,
          Guid.Parse(subscriptionUidAssigned), customHeaders, subscriptionProxy).ConfigureAwait(false);
      }

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
  }
}
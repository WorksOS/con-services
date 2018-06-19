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
  /// The executor which updates a project - appropriate for v4 controller
  /// </summary>
  public class UpdateProjectExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Save for potential rollback
    /// </summary>
    protected string subscriptionUidAssigned;

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
      ProjectRequestHelper.ValidateGeofence(updateProjectEvent.ProjectBoundary, serviceExceptionHandler);

      // if updating  type from Standard to Landfill, or creating Landfill, then must have a CoordSystem
      ProjectRequestHelper.ValidateCoordSystemFile(existing, updateProjectEvent, serviceExceptionHandler);

      await ProjectRequestHelper.ValidateCoordSystemInRaptor(updateProjectEvent,
        serviceExceptionHandler, customHeaders, raptorProxy).ConfigureAwait(false);
      
      if (!string.IsNullOrEmpty(updateProjectEvent.ProjectBoundary) && String.Compare(existing.GeometryWKT, updateProjectEvent.ProjectBoundary, StringComparison.OrdinalIgnoreCase) != 0)
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
        await ProjectDataValidator.ValidateFreeSub(customerUid, updateProjectEvent.ProjectType,
          log, serviceExceptionHandler, subscriptionRepo);
      }
      log.LogDebug($"UpdateProject: passed validation {updateProjectEvent.ProjectUID}");

      /*** now making changes, potentially needing rollback ***/
      //  order changes to minimise rollback
      //    if CreateCoordSystemInRaptorAndTcc fails then nothing is done
      //    if AssociateProjectSubscription fails then nothing is done
      //    if UpdateProjectEvent fails then ProjectSubscription is Dissassociated
      //    if UpdateGeofenceInGeofenceService fails then ProjectSubscription is Dissassociated 
      if (!string.IsNullOrEmpty(updateProjectEvent.CoordinateSystemFileName))
      {
        await ProjectRequestHelper.CreateCoordSystemInRaptorAndTcc(updateProjectEvent.ProjectUID, existing.LegacyProjectID,
          updateProjectEvent.CoordinateSystemFileName, updateProjectEvent.CoordinateSystemFileContent, false,
          log, serviceExceptionHandler, customerUid, customHeaders,
          projectRepo, raptorProxy, configStore, fileRepo).ConfigureAwait(false);
      }

      if (existing != null && existing.ProjectType == ProjectType.Standard &&
          ( updateProjectEvent.ProjectType == ProjectType.LandFill || updateProjectEvent.ProjectType == ProjectType.ProjectMonitoring))
        {
          subscriptionUidAssigned = await ProjectRequestHelper.AssociateProjectSubscriptionInSubscriptionService(
              updateProjectEvent.ProjectUID.ToString(), updateProjectEvent.ProjectType, customerUid,
              log, serviceExceptionHandler, customHeaders, subscriptionProxy, subscriptionRepo, projectRepo, false)
            .ConfigureAwait(false);
        }

      var isUpdated = await projectRepo.StoreEvent(updateProjectEvent).ConfigureAwait(false);
      if (isUpdated == 0)
      {
        if (!string.IsNullOrEmpty(subscriptionUidAssigned))
          await ProjectRequestHelper.DissociateProjectSubscription(updateProjectEvent.ProjectUID, Guid.Parse(subscriptionUidAssigned), customHeaders, subscriptionProxy).ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 62);
      }

      if (!string.IsNullOrEmpty(updateProjectEvent.ProjectBoundary) && String.Compare(existing.GeometryWKT, updateProjectEvent.ProjectBoundary, StringComparison.OrdinalIgnoreCase) != 0)
      {
        await UpdateGeofenceInGeofenceService(updateProjectEvent);
        if (!string.IsNullOrEmpty(subscriptionUidAssigned))
          await ProjectRequestHelper.DissociateProjectSubscription(updateProjectEvent.ProjectUID, Guid.Parse(subscriptionUidAssigned), customHeaders, subscriptionProxy).ConfigureAwait(false);
      }

      log.LogDebug("CreateProjectV4. completed succesfully");
      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }


    /// <summary>
    /// Updates the projectType geofence
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    protected async Task UpdateGeofenceInGeofenceService(UpdateProjectEvent project)
    {
      log.LogDebug($"Updating the Project geofence: {JsonConvert.SerializeObject(project.ProjectUID)}");

      var projectGeofence = await GetProjectGeofence(project.ProjectUID.ToString()).ConfigureAwait(false);
      if (projectGeofence == null)
      {
        if (!string.IsNullOrEmpty(subscriptionUidAssigned))
          await ProjectRequestHelper.DissociateProjectSubscription(project.ProjectUID, Guid.Parse(subscriptionUidAssigned), customHeaders, subscriptionProxy).ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 96, "UpdateGeofenceInGeofenceService: Unable to find the project-geofence association.");
      }

      GeofenceData geofence = null;
      try
      {
        geofence = await geofenceProxy.GetGeofenceForCustomer(customerUid, projectGeofence.GeofenceUID, customHeaders).ConfigureAwait(false);
        if (geofence == null)
        {
          if (!string.IsNullOrEmpty(subscriptionUidAssigned))
            await ProjectRequestHelper.DissociateProjectSubscription(project.ProjectUID, Guid.Parse(subscriptionUidAssigned), customHeaders, subscriptionProxy).ConfigureAwait(false);

          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 97, "UpdateGeofenceInGeofenceService: Unable to find the projects Geofence.");
        }
      }
      catch (Exception e)
      {
        if (!string.IsNullOrEmpty(subscriptionUidAssigned))
          await ProjectRequestHelper.DissociateProjectSubscription(project.ProjectUID, Guid.Parse(subscriptionUidAssigned), customHeaders, subscriptionProxy).ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 98, e.Message);
      }

      Guid geofenceUidUpdated = Guid.Empty;
      try
      {
        var area = GeofenceValidation.CalculateAreaSqMeters(project.ProjectBoundary);
        geofenceUidUpdated = await geofenceProxy.UpdateGeofence(geofence.GeofenceUID, Guid.Parse(customerUid), geofence.GeofenceName,
          geofence.Description, geofence.GeofenceType,
          project.ProjectBoundary,
          geofence.FillColor, geofence.IsTransparent, geofence.UserUID, area, customHeaders).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        if (!string.IsNullOrEmpty(subscriptionUidAssigned))
          await ProjectRequestHelper.DissociateProjectSubscription(project.ProjectUID, Guid.Parse(subscriptionUidAssigned), customHeaders, subscriptionProxy).ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 100, e.Message);
      }

      if (geofenceUidUpdated == Guid.Empty)
      {
        if (!string.IsNullOrEmpty(subscriptionUidAssigned))
          await ProjectRequestHelper.DissociateProjectSubscription(project.ProjectUID, Guid.Parse(subscriptionUidAssigned), customHeaders, subscriptionProxy).ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 99);
      }
    }

    private async Task<ProjectGeofence> GetProjectGeofence(string projectUid)
    {
      log.LogInformation($"GetProjectGeofence by projectUid: {projectUid}");

      var projectGeofence =
        (await projectRepo.GetAssociatedGeofences(projectUid).ConfigureAwait(false)).FirstOrDefault(
          p => p.GeofenceType == GeofenceType.Project);
      
      if (projectGeofence == null)
      {
        if (!string.IsNullOrEmpty(subscriptionUidAssigned))
          await ProjectRequestHelper.DissociateProjectSubscription(Guid.Parse(projectUid), Guid.Parse(subscriptionUidAssigned), customHeaders, subscriptionProxy).ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 101, "geofenceProxy.GetProjectGeofence failed");
      }

      log.LogInformation($"Project geofenceUid: {projectGeofence.GeofenceUID} retrieved");
      return projectGeofence;
    }

  }
}
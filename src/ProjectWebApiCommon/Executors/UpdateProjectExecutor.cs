using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
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

      ProjectRequestHelper.ValidateProjectBoundary(updateProjectEvent.ProjectBoundary, serviceExceptionHandler);

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
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

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
    protected string subscriptionUidAssigned;

    /// <summary>
    /// Processes the UpdateProjectEvent
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var updateProjectEvent = CastRequestObjectTo<UpdateProjectEvent>(item, errorCode: 68);

      var existing = await projectRepo.GetProject(updateProjectEvent.ProjectUID.ToString());
      if (existing == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 7);

      ProjectRequestHelper.ValidateProjectBoundary(updateProjectEvent.ProjectBoundary, serviceExceptionHandler);

      await ProjectRequestHelper.ValidateCoordSystemInProductivity3D(updateProjectEvent, serviceExceptionHandler, customHeaders, productivity3dV1ProxyCoord).ConfigureAwait(false);

      // todoMaverick theres a bug if endDate is extended, it needs to re-check overlap
      if (!string.IsNullOrEmpty(updateProjectEvent.ProjectBoundary) && string.Compare(existing.GeometryWKT,
            updateProjectEvent.ProjectBoundary, StringComparison.OrdinalIgnoreCase) != 0)
      {
        await ProjectRequestHelper.DoesProjectOverlap(existing.CustomerUID, updateProjectEvent.ProjectUID.ToString(),
          existing.StartDate, updateProjectEvent.ProjectEndDate, updateProjectEvent.ProjectBoundary,
          log, serviceExceptionHandler, projectRepo);
      }

      if (existing != null && existing.ProjectType != updateProjectEvent.ProjectType)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 7); // todoMaverick only 1 projectType now

      log.LogDebug($"UpdateProject: passed validation {updateProjectEvent.ProjectUID}");

      /*** now making changes, potentially needing rollback ***/
      //  order changes to minimise rollback
      //    if CreateCoordSystemInProductivity3dAndTcc fails then nothing is done
      //    if AssociateProjectSubscription fails then nothing is done
      //    if UpdateProjectEvent fails then ProjectSubscription is Dissassociated
      if (!string.IsNullOrEmpty(updateProjectEvent.CoordinateSystemFileName))
      {
        // don't bother rolling this back
        await ProjectRequestHelper.CreateCoordSystemInProductivity3dAndTcc(updateProjectEvent.ProjectUID,
          existing.ShortRaptorProjectId,
          updateProjectEvent.CoordinateSystemFileName, updateProjectEvent.CoordinateSystemFileContent, false,
          log, serviceExceptionHandler, customerUid, customHeaders,
          projectRepo, productivity3dV1ProxyCoord, configStore, fileRepo, dataOceanClient, authn).ConfigureAwait(false);
        log.LogDebug("UpdateProject: CreateCoordSystemInProductivity3dAndTcc succeeded");
      }

      // todoMaverick if actually creating, then need to wrote to WM first to obtain the ProjectUid (and accountTrn?) for our DB 
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

      log.LogDebug("UpdateProject: Project updated successfully");
      return new ContractExecutionResult();
    }


    private async Task RollbackAndThrow(UpdateProjectEvent updateProjectEvent, HttpStatusCode httpStatusCode, int errorCode, string exceptionMessage, ProjectDatabaseModel existing = null)
    {
      log.LogDebug($"Rolling back the Project Update for updateProjectEvent: {updateProjectEvent.ProjectUID.ToString()} subscriptionUidAssigned: {subscriptionUidAssigned}");
      
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

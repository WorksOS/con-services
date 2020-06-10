using System;
using System.Net;
using System.Threading.Tasks;
using CCSS.Geometry;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which updates a project in cws, storing in trex/DO as appropriate
  /// </summary>
  public class UpdateProjectExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the UpdateProjectEvent
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var updateProjectEvent = CastRequestObjectTo<UpdateProjectEvent>(item, errorCode: 68);

      var existing = await ProjectRequestHelper.GetProject(
        updateProjectEvent.ProjectUID, new Guid(customerUid), new Guid(userId),
        log, serviceExceptionHandler, cwsProjectClient, customHeaders);
      if (existing == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 7);

      await ProjectRequestHelper.ValidateCoordSystemInProductivity3D(
        updateProjectEvent.CoordinateSystemFileName, updateProjectEvent.CoordinateSystemFileContent,
        serviceExceptionHandler, customHeaders, productivity3dV1ProxyCoord).ConfigureAwait(false);

      if (!string.IsNullOrEmpty(updateProjectEvent.ProjectBoundary) && string.Compare(existing.Boundary,
        updateProjectEvent.ProjectBoundary, StringComparison.OrdinalIgnoreCase) != 0)
      {
        await ProjectRequestHelper.DoesProjectOverlap(new Guid(existing.CustomerUID), updateProjectEvent.ProjectUID, new Guid(userId),
          updateProjectEvent.ProjectBoundary, log, serviceExceptionHandler, cwsProjectClient, customHeaders);
      }

      if (existing != null && existing.ProjectType != updateProjectEvent.ProjectType)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 7);

      log.LogDebug($"{nameof(UpdateProjectExecutor)}: passed validation {updateProjectEvent.ProjectUID}");

      // create/update in Cws
      try
      {
        // CCSSSCON-214 what kind of errors?
        var projectUid = await UpdateCws(existing, updateProjectEvent);
        if (!string.IsNullOrEmpty(projectUid)) // no error, may have been a create project
          updateProjectEvent.ProjectUID = new Guid(projectUid);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 62, "worksManager.UpdateProject", e.Message);
      }

      // now making changes, potentially needing rollback 
      //  order changes to minimize rollback. If any fails, then project is deleted in cws
      //    if CreateCoordSystem 3dp/Trex fails 
      //    if tcc and DO write fails
      if (!string.IsNullOrEmpty(updateProjectEvent.CoordinateSystemFileName))
      {
        await ProjectRequestHelper.CreateCoordSystemInProductivity3dAndTcc(updateProjectEvent.ProjectUID,
          updateProjectEvent.CoordinateSystemFileName, updateProjectEvent.CoordinateSystemFileContent, false,
          log, serviceExceptionHandler, customerUid, customHeaders,
          productivity3dV1ProxyCoord, configStore, fileRepo, dataOceanClient, authn,
          cwsDesignClient, cwsProfileSettingsClient, cwsProjectClient).ConfigureAwait(false);
        log.LogDebug($"{nameof(UpdateProjectExecutor)}: CreateCoordSystemInProductivity3dAndTcc succeeded");
      }
      log.LogDebug($"{nameof(UpdateProjectExecutor)}: Project updated successfully");
      return new ContractExecutionResult();
    }

    // if actually creating, then need to write to WM first to obtain the ProjectUid for our DB 
    private async Task<string> UpdateCws(ProjectDatabaseModel existing, UpdateProjectEvent updateProjectEvent)
    {
      if (existing == null)
      {
        try
        {
          var createProjectRequestModel = AutoMapperUtility.Automapper.Map<CreateProjectRequestModel>(updateProjectEvent);
          createProjectRequestModel.AccountId = customerUid;
          createProjectRequestModel.Boundary = GeometryConversion.MapProjectBoundary(updateProjectEvent.ProjectBoundary);

          var response = await cwsProjectClient.CreateProject(createProjectRequestModel, customHeaders);
          if (response != null)
          {
            // CCSSSCON-214 what about exception/other error
            updateProjectEvent.ProjectUID = new Guid(response.Id);
            return response.Id;
          }
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 62, "worksManager.CreateProject", e.Message);
        }
      }
      else
      {
        if (string.Compare(existing.Name, updateProjectEvent.ProjectName, true) != 0)
        {
          // CCSSSCON-214 what are errors?
          var updateProjectDetailsRequestModel = new UpdateProjectDetailsRequestModel() { projectName = updateProjectEvent.ProjectName };
          await cwsProjectClient.UpdateProjectDetails(updateProjectEvent.ProjectUID, updateProjectDetailsRequestModel, customHeaders);
        }
        if (!string.IsNullOrEmpty(updateProjectEvent.ProjectBoundary) && string.Compare(existing.Boundary,
            updateProjectEvent.ProjectBoundary, StringComparison.OrdinalIgnoreCase) != 0)
        {
          // CCSSSCON-214 what are errors?
          var boundary = GeometryConversion.MapProjectBoundary(updateProjectEvent.ProjectBoundary);
          await cwsProjectClient.UpdateProjectBoundary(updateProjectEvent.ProjectUID, boundary, customHeaders);
        }
        return updateProjectEvent.ProjectUID.ToString();
      }
      return null;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}

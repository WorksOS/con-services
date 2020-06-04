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

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which creates a project - appropriate for v2 and v4 controllers
  /// </summary>
  public class CreateProjectExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the CreateProjectEvent
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var createProjectEvent = CastRequestObjectTo<CreateProjectEvent>(item, errorCode: 68);

      await ProjectRequestHelper.ValidateCoordSystemInProductivity3D(
        createProjectEvent.CoordinateSystemFileName, createProjectEvent.CoordinateSystemFileContent,
        serviceExceptionHandler, customHeaders, productivity3dV1ProxyCoord).ConfigureAwait(false);

      // Write to WM first to obtain their ProjectTRN to use as ProjectUid for our DB etc
      try
      {
        // don't send our timezone, we only need it for WorksOS. WM has their own, calculated from the boundary, for their own uses.
        var createProjectRequestModel = AutoMapperUtility.Automapper.Map<CreateProjectRequestModel>(createProjectEvent);
        createProjectRequestModel.Boundary = GeometryConversion.MapProjectBoundary(createProjectEvent.ProjectBoundary);

        // CCSSSCON-141 what are exceptions/other error
        var response = await cwsProjectClient.CreateProject(createProjectRequestModel, customHeaders);
        if (response != null && !string.IsNullOrEmpty(response.Id))
          createProjectEvent.ProjectUID = new Guid(response.Id);
        else
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 7);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 61, "worksManager.CreateProject", e.Message);
      }

      // now making changes, potentially needing rollback 
      //  order changes to minimize rollback. If any fails, then project is deleted in cws
      //    if CreateCoordSystem 3dp/Trex fails 
      //    if tcc and DO write fails
      await ProjectRequestHelper.CreateCoordSystemInProductivity3dAndTcc(
        createProjectEvent.ProjectUID, createProjectEvent.CoordinateSystemFileName,
        createProjectEvent.CoordinateSystemFileContent, true, log, serviceExceptionHandler, customerUid, customHeaders,
        productivity3dV1ProxyCoord, configStore, fileRepo, dataOceanClient, authn,
        cwsDesignClient, cwsProfileSettingsClient, cwsProjectClient).ConfigureAwait(false);
      log.LogDebug($"CreateProject: Created project {createProjectEvent.ProjectUID}");

      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item) => throw new NotImplementedException();

  }
}

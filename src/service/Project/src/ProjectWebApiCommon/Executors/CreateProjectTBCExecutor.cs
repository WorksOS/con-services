using System;
using System.Net;
using System.Threading.Tasks;
using CCSS.Geometry;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which creates a project - only used for TBC now, and that is temporary
  /// </summary>
  public class CreateProjectTBCExecutor : RequestExecutorContainer
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
      log.LogDebug($"{nameof(CreateProjectTBCExecutor)}: Created project {createProjectEvent.ProjectUID}");

      // Send to whoever needs it
      await ProjectRequestHelper.DispenseCopiesOfCoordSystem(
        createProjectEvent.ProjectUID, createProjectEvent.CoordinateSystemFileName,
        createProjectEvent.CoordinateSystemFileContent, true, log, serviceExceptionHandler, customerUid, customHeaders,
        productivity3dV1ProxyCoord, configStore, dataOceanClient, authn,
        cwsDesignClient, cwsProfileSettingsClient, cwsProjectClient).ConfigureAwait(false);

      return new ProjectV6DescriptorsSingleResult(
        AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(await ProjectRequestHelper.GetProject(createProjectEvent.ProjectUID, new Guid(customerUid), new Guid(userId),
            log, serviceExceptionHandler, cwsProjectClient, customHeaders)
          .ConfigureAwait(false)));
    }

    protected override ContractExecutionResult ProcessEx<T>(T item) => throw new NotImplementedException();

  }
}

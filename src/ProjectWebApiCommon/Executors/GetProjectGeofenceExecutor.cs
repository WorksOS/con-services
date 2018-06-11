using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which gets the geofences for the customer and/or project
  /// </summary>
  public class GetProjectGeofenceExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the GetProjectGeofence request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ProjectGeofenceResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      //ContractExecutionResult result = null;
      //ProjectGeofenceRequest projectGeofenceRequest = item as ProjectGeofenceRequest;
      //if (projectGeofenceRequest == null )
      //  serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68 /* todo */);

      //if (projectGeofenceRequest.ProjectSettingsType != ProjectSettingsType.Targets && projectGeofenceRequest.ProjectSettingsType != ProjectSettingsType.Colors)
      //  serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 77);

      //await ValidateProjectWithCustomer(customerUid, projectGeofenceRequest.projectUid);

      //try
      //{
      //  var projectGeofence = await projectRepo.GetProjectSettings(projectGeofenceRequest.projectUid, userId, projectGeofenceRequest.ProjectSettingsType).ConfigureAwait(false);

      //  result = projectSettings == null ? 
      //    ProjectGeofenceResult.CreateProjectGeofenceResult(projectGeofenceRequest.projectUid, null, projectGeofenceRequest.ProjectSettingsType) :
      //    ProjectGeofenceResult.CreateProjectGeofenceResult(projectGeofence.ProjectUid, JsonConvert.DeserializeObject<JObject>(projectGeofence.Settings), projectSettings.ProjectSettingsType);
      //}
      //catch (Exception e)
      //{
      //  serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
      //}
      return new ContractExecutionResult();
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
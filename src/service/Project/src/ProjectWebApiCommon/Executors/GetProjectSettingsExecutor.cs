using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which gets the project settings for the project
  /// </summary>
  public class GetProjectSettingsExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the GetProjectSettings request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ContractExecutionResult result = null;

      var projectSettingsRequest = CastRequestObjectTo<ProjectSettingsRequest>(item, errorCode: 68);

      if (projectSettingsRequest.ProjectSettingsType != ProjectSettingsType.Targets && projectSettingsRequest.ProjectSettingsType != ProjectSettingsType.Colors)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 77);

      await ValidateProjectWithCustomer(customerUid, projectSettingsRequest.projectUid);

      try
      {
        var projectSettings = await projectRepo.GetProjectSettings(projectSettingsRequest.projectUid, userId, projectSettingsRequest.ProjectSettingsType).ConfigureAwait(false);

        result = projectSettings == null ?
          ProjectSettingsResult.CreateProjectSettingsResult(projectSettingsRequest.projectUid, null, projectSettingsRequest.ProjectSettingsType) :
          ProjectSettingsResult.CreateProjectSettingsResult(projectSettings.ProjectUid, JsonConvert.DeserializeObject<JObject>(projectSettings.Settings), projectSettings.ProjectSettingsType);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
      }
      return result;
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}

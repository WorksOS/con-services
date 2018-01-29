using System;
using System.Net;
using System.Threading.Tasks;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ProjectSettingsResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ContractExecutionResult result = null;
      ProjectSettingsRequest projectSettingsRequest = item as ProjectSettingsRequest;
      if ( projectSettingsRequest == null )
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);
      await ValidateProjectWithCustomer(customerUid, projectSettingsRequest.projectUid);

      try
      {
        var projectSettings = await projectRepo.GetProjectSettings(projectSettingsRequest.projectUid, userId, projectSettingsRequest.ProjectSettingsType).ConfigureAwait(false);

        result = projectSettings == null ? 
          ProjectSettingsResult.CreateProjectSettingsResult(projectSettingsRequest.projectUid, null, projectSettingsRequest.ProjectSettingsType) : 
          ProjectSettingsResult.CreateProjectSettingsResult(projectSettings.ProjectUid, projectSettings.Settings, projectSettings.ProjectSettingsType);
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
    protected override void ProcessErrorCodes()
    {
    }
  }
}
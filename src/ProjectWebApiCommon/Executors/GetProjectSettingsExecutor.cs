using System;
using System.Net;
using System.Threading.Tasks;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which gets the project settings for the project
  /// </summary>
  public class GetProjectSettingsExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the GetProjectSettings request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ProjectSettingsResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ContractExecutionResult result = null;
      ProjectSettingsRequest projectSettingsRequest = null;
      
      projectSettingsRequest = item as ProjectSettingsRequest;
      if ( projectSettingsRequest == null )
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);
      await ValidateProjectWithCustomer(customerUid, projectSettingsRequest?.projectUid);

      try
      {
        var projectSettings = await projectRepo.GetProjectSettings(projectSettingsRequest?.projectUid).ConfigureAwait(false);
        result = ProjectSettingsResult.CreateProjectSettingsResult(projectSettingsRequest?.projectUid, projectSettings?.Settings);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
      }
      return result;
    }

  }
}
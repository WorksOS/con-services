using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.VisionLink.Interfaces.Events.Preference;

namespace CCSS.Productivity3D.Preferences.Common.Executors
{
  public class DeleteUserPreferenceExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the DeleteUserPreference request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var deleteUserPrefEvent = CastRequestObjectTo<DeleteUserPreferenceEvent>(item, errorCode: 1);

      var isDeleted = 0;
      try
      {
        isDeleted = await preferenceRepo.StoreEvent(deleteUserPrefEvent);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 12, "preferenceRepo.storeDeleteUserPreference", e.Message);
      }

      if (isDeleted == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 12);

      log.LogDebug("DeleteUserPreference completed successfully");
      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class ProjectSettingsRequestHelper : DataRequestBase, IProjectSettingsRequestHelper
  {
    public ProjectSettingsRequestHelper()
    { }

    public ProjectSettingsRequestHelper(ILoggerFactory logger)
     {
      log = logger.CreateLogger<ProjectSettingsRequestHelper>();
    }

    /// <summary>
    /// Creates an instance of the ProjectSettingsRequest class and populate it.   
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="settings"></param>
    /// <param name="projectSettingsType"></param>
    /// <returns>An instance of the ProjectSettingsRequest class.</returns>
    public ProjectSettingsRequest CreateProjectSettingsRequest(string projectUid, string settings, ProjectSettingsType projectSettingsType)
    {
      return ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, projectSettingsType);
    }

    public async Task RaptorValidateProjectSettings(
      IRaptorProxy raptorProxy, ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      ProjectSettingsRequest request, IDictionary<string, string> customHeaders
      )
    {
      BaseDataResult result = null;
      try
      {
        result = await raptorProxy
          .ValidateProjectSettings(request, customHeaders)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"RaptorValidateProjectSettings: RaptorServices failed with exception. projectUid:{request.projectUid} settings:{request.Settings}. Exception Thrown: {e.Message}. ");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 70,
          "raptorProxy.ValidateProjectSettings", e.Message);
      }

      log.LogDebug(
        $"RaptorValidateProjectSettings: projectUid: {request.projectUid} settings: {request.Settings}. RaptorServices returned code: {result?.Code ?? -1} Message {result?.Message ?? "result == null"}.");

      if (result != null && result.Code != 0)
      {
        log.LogError(
          $"RaptorValidateProjectSettings: RaptorServices failed. projectUid:{request.projectUid} settings:{request.Settings}. Reason: {result?.Code ?? -1} {result?.Message ?? "null"}. ");

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 67, result.Code.ToString(),
          result.Message);
      }
    }
  }
}

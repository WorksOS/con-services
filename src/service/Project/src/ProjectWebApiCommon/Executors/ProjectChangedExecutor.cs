using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// Handles notifications from CWS that a project has changed.
  /// </summary>
  public class ProjectChangedExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var data = CastRequestObjectTo<ProjectChangeNotificationDto>(item, errorCode: 68);

      //Check customerUid in request matches header since some of the API calls use the data and some the header
      var customerUid = string.IsNullOrEmpty(data.AccountTrn) ? null : TRNHelper.ExtractGuid(data.AccountTrn);
      if (customerUid.HasValue && customerUid.ToString() != customHeaders["X-VisionLink-CustomerUID"])
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 135);
      }

      var projectUid = string.IsNullOrEmpty(data.ProjectTrn) ? null : TRNHelper.ExtractGuid(data.ProjectTrn);

      if (data.NotificationType.HasFlag(NotificationType.CoordinateSystem))
      {
        if (projectUid.HasValue)
          await SaveCoordinateSystem(projectUid.Value, data.CoordinateSystemFileName, data.CoordinateSystemFileContent);
        else
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 5);
        }
      }

      if (data.NotificationType.HasFlag(NotificationType.MetaData))
      {
        if (projectUid.HasValue)
        {
          log.LogInformation($"Clearing cache related to Project ID: {projectUid.Value}");
          await notificationHubClient.Notify(new ProjectChangedNotification(projectUid.Value));
        }

        if (customerUid.HasValue)
        {
          log.LogInformation($"Clearing cache related to Customer ID: {customerUid.Value}");
          await notificationHubClient.Notify(new CustomerChangedNotification(customerUid.Value));
        }
      }
      return new ContractExecutionResult();
    }

    /// <summary>
    /// Pass Coordinate System to TRex and save a copy in DataOcean for DXF tile generation.
    /// </summary>
    private async Task SaveCoordinateSystem(Guid projectUid,
      string coordinateSystemFileName,
      byte[] coordinateSystemFileContent)
    {
      //Save to DataOcean for DXF tile generation
      var rootFolder = configStore.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      if (string.IsNullOrEmpty(rootFolder))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 115);
      }

      using (var ms = new MemoryStream(coordinateSystemFileContent))
      {
        await DataOceanHelper.WriteFileToDataOcean(
          ms, rootFolder, customerUid, projectUid.ToString(),
          DataOceanFileUtil.DataOceanFileName(coordinateSystemFileName, false, projectUid, null),
          log, serviceExceptionHandler, dataOceanClient, authn, projectUid, configStore);
      }

      //Save in TRex
      CoordinateSystemSettingsResult coordinateSystemSettingsResult = await productivity3dV1ProxyCoord
        .CoordinateSystemPost(projectUid,
          coordinateSystemFileContent, coordinateSystemFileName, customHeaders);
      var message = string.Format($"Sending coordinate system to TRex returned code: {0} Message {1}.",
        coordinateSystemSettingsResult?.Code ?? -1,
        coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null");
      log.LogDebug(message);
      if (coordinateSystemSettingsResult == null ||
          coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
      {
        log.LogCritical($"Failed to save coordinate system file in TRex for project {projectUid}");
      }
     
    }

    protected override ContractExecutionResult ProcessEx<T>(T item) => throw new NotImplementedException();

  }
}

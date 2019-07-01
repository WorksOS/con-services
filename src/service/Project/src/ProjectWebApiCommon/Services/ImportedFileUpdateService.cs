using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.Productivity.Push.Models.Attributes;
using VSS.Productivity.Push.Models.Enums;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;

namespace VSS.MasterData.Project.WebAPI.Common.Services
{
  /// <summary>
  /// Handles push notifications for imported file updates
  /// </summary>
  public class ImportedFileUpdateService
  {
    private readonly  ILogger log;
    private readonly IProjectRepository projectRepo;
    private readonly IServiceExceptionHandler serviceExceptionHandler;

    public ImportedFileUpdateService(ILoggerFactory logger, IProjectRepository projectRepo, IServiceExceptionHandler serviceExceptionHandler)
    {
      this.projectRepo = projectRepo;
      this.serviceExceptionHandler = serviceExceptionHandler;
      log = logger.CreateLogger<ImportedFileUpdateService>();
    }

    /// <summary>
    /// Handles the notification for DXF tiles having been generated
    /// </summary>
    [Notification(NotificationUidType.File, ProjectFileRasterTilesGeneratedNotification.PROJECT_FILE_RASTER_TILES_GENERATED_KEY)]
    public async Task UpdateZoomLevelsInDatabase(object parameters)
    {
      RasterTileNotificationParameters result;
      try
      {
          result = JObject.FromObject(parameters).ToObject<RasterTileNotificationParameters>();
      }
      catch (Exception e)
      {
        log.LogError(e, "Bad parameters passed to generated DXF tiles notification");
        /*
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "Wrong parameters passed to generated DXF tiles notification"));
            */
        return;
      }

      log.LogInformation($"Received {ProjectFileRasterTilesGeneratedNotification.PROJECT_FILE_RASTER_TILES_GENERATED_KEY} notification: {JsonConvert.SerializeObject(result)}");

      var existing = await projectRepo.GetImportedFile(result.FileUid.ToString());
      //Check file is still there, user may have deleted it in the meanwhile
      if (existing != null)
      {
        await ImportedFileRequestDatabaseHelper.UpdateImportedFileInDb(
          existing, existing.FileDescriptor, existing.SurveyedUtc, result.MinZoomLevel, result.MaxZoomLevel,
          existing.FileCreatedUtc, existing.FileUpdatedUtc, existing.ImportedBy, log, serviceExceptionHandler, projectRepo);
      }
    }
  }
}

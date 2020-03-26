using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.Productivity3D.Productivity3D.Models.Notification.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  public interface ISubscriberService
  {
    Task AddFileProcessed(AddFileResult result);
  }

  public class SubscriberService : ISubscriberService, ICapSubscribe
  {
    private readonly IProjectRepository projectRepo;
    private readonly ILogger log;
    private readonly IServiceExceptionHandler serviceExceptionHandler;

    public SubscriberService(IProjectRepository projectRepo, IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler)
    {
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.projectRepo = projectRepo;
      log = logger.CreateLogger<SubscriberService>();
    }

    //Disable CAP for now #76666
    //[CapSubscribe("VSS.Productivity3D.Service.AddFileProcessedEvent-Dev")]
    //[CapSubscribe("VSS.Productivity3D.Service.AddFileProcessedEvent-Alpha")]
    //[CapSubscribe("VSS.Productivity3D.Service.AddFileProcessedEvent-3dpm")]
    public async Task AddFileProcessed(AddFileResult result)
    {
      log.LogInformation($"Received AddFileProcessedEvent from CAP for fileUid {result.FileUid}");
      var existing = await projectRepo.GetImportedFile(result.FileUid.ToString())
        .ConfigureAwait(false);

      if (existing == null)
      {
        log.LogWarning($"Failed to find file {result.FileUid} in database. Cannot update zoom levels.");
      }
      else
      {
        var updateImportedFileEvent = await ImportedFileRequestDatabaseHelper.UpdateImportedFileInDb(existing,
            JsonConvert.SerializeObject(result.FileDescriptor),
            existing.SurveyedUtc, result.MinZoomLevel, result.MaxZoomLevel,
            existing.FileCreatedUtc, existing.FileUpdatedUtc, result.UserEmailAddress,
            log, serviceExceptionHandler, projectRepo)
          .ConfigureAwait(false);

      }
    }
  }
}

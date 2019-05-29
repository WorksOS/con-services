using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Push.Abstractions.Notifications;
#if RAPTOR
using VSS.Productivity3D.WebApi.Models.Notification.Executors;
#endif
using VSS.Productivity3D.WebApiModels.Notification.Models;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.WebApi.Models.Services
{
  public interface IEnqueueItem<in T>
  {
    bool EnqueueItem(T item);
  }

  public class AddFileProcessingService : IHostedService, IEnqueueItem<ProjectFileDescriptor>
  {
    private readonly ConcurrentQueue<ProjectFileDescriptor> queue = new ConcurrentQueue<ProjectFileDescriptor>();
    private ILogger<AddFileProcessingService> log;
    private readonly IConfigurationStore configServiceStore;
    private readonly IFileRepository fileRepo;
#if RAPTOR
    private readonly IASNodeClient raptorServiceClient;
#endif
    private readonly ILoggerFactory loggingFactory;
    private readonly ITileGenerator tileServiceGenerator;
    private CancellationToken token;
    private bool stopRequested = false;
    private SemaphoreSlim stopSemaphore = new SemaphoreSlim(1);
    private readonly INotificationHubClient notificationHubClient;


    public AddFileProcessingService(ILogger<AddFileProcessingService> logger, ILoggerFactory logFactory,
      IConfigurationStore configService, IFileRepository repositoryService,
#if RAPTOR
      IASNodeClient raptorService,
#endif
      ITileGenerator tileService,
      INotificationHubClient notificationHubClient)
    {
      log = logger;
      configServiceStore = configService;
      fileRepo = repositoryService;
      loggingFactory = logFactory;
#if RAPTOR
      raptorServiceClient = raptorService;
#endif
      tileServiceGenerator = tileService;
      this.notificationHubClient = notificationHubClient;
    }

    private async Task<AddFileResult> ProcessItem(ProjectFileDescriptor file)
    {
#if RAPTOR
      var executor = RequestExecutorContainerFactory.Build<AddFileExecutor>(loggingFactory, raptorServiceClient, null,
        configServiceStore, fileRepo, tileServiceGenerator, null, null, null, null, null);
      var result = await executor.ProcessAsync(file) as AddFileResult;
      log.LogInformation($"Processed file {file.File.FileName} with result {JsonConvert.SerializeObject(result)}");
      var eventAttributes = new Dictionary<string, object>
      {
        {"file", file.File.FileName},
        {"status", result.Code.ToString() },
        {"result", result.Message }
      };
      NewRelic.Api.Agent.NewRelic.RecordCustomEvent("3DPM_Request_files", eventAttributes);
      log.LogInformation($"Pushing DXF tile notification for {result.FileUid}");
      var notifyParams = new DxfTileNotificationParameters
      {
        FileUid = result.FileUid,
        MinZoomLevel = result.MinZoomLevel,
        MaxZoomLevel = result.MaxZoomLevel
      };
      await notificationHubClient.Notify(new ProjectFileDxfTilesGeneratedNotification(notifyParams));
      return result;
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    public void StartSpinCycle(CancellationToken cancellationToken)
    {
      log.LogInformation("Starting file processing thread");
      token = cancellationToken;
      var spinnerThread = new Thread(StartThread);
      spinnerThread.Start();
    }

    private void StartThread()
    {
      stopSemaphore.Wait(token);
      while (!token.IsCancellationRequested && !stopRequested)
      {
        if (queue.Count > 0)
        {
          if (queue.TryDequeue(out var descriptor))
          {
            log.LogInformation($"Processing file {JsonConvert.SerializeObject(descriptor)}");
            var task = ProcessItem(descriptor);
          }
        }
        else
          Thread.Sleep(500);
      }
      stopSemaphore.Release();
      log.LogInformation($"Stopped file processing thread");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      StartSpinCycle(cancellationToken);
      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      log.LogInformation($"Stopping file processing thread");
      stopRequested = true;
      return stopSemaphore.WaitAsync(cancellationToken);
    }

    public bool EnqueueItem(ProjectFileDescriptor item)
    {
      queue.Enqueue(item);
      return true;
    }
  }
}

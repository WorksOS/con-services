using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Pegasus.Client;
using VSS.Pegasus.Client.Models;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.Productivity3D.Scheduler.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.Jobs.DxfTileJob
{
  public abstract class TileGenerationJob<T> : IJob where T : TileGenerationRequest
  {
    public abstract Guid VSSJobUid { get; }

    protected readonly IPegasusClient pegasusClient;
    private readonly ITPaaSApplicationAuthentication authentication;
    private readonly INotificationHubClient notificationHubClient;
    protected readonly ILogger log;
    private readonly IConfigurationStore configStore;

    protected PerformContext jobContext;

    public TileGenerationJob(IConfigurationStore configurationStore, IPegasusClient pegasusClient, ITPaaSApplicationAuthentication authn, INotificationHubClient notificationHubClient, ILoggerFactory logger)
    {
      configStore = configurationStore;
      this.pegasusClient = pegasusClient;
      authentication = authn;
      this.notificationHubClient = notificationHubClient;
      log = logger.CreateLogger<DxfTileGenerationJob>();
    }

    public Task Setup(object o, object context) => Task.FromResult(true);

    public async Task Run(object o, object context)
    {
      var request = o.GetConvertedObject<T>();

      if (context is PerformContext performContext)
      {
        jobContext = performContext;
      }

      //Validate the parameters
      request.Validate();

      if (configStore.GetValueBool("SCHEDULER_ENABLE_DXF_TILE_GENERATION").HasValue &&
          configStore.GetValueBool("SCHEDULER_ENABLE_DXF_TILE_GENERATION").Value)
      {
        log.LogInformation($"Tile generation with Pegasus enabled: {JsonConvert.SerializeObject(request)}");

        var result = await GenerateTiles(request);

        log.LogInformation($"Received Pegasus response for tile generation, filename: {request.FileName}, result: `{JsonConvert.SerializeObject(result)}`");

        var notifyParams = new RasterTileNotificationParameters
        {
          FileUid = request.ImportedFileUid,
          MinZoomLevel = result.MinZoom,
          MaxZoomLevel = result.MaxZoom
        };

        await notificationHubClient.Notify(new ProjectFileRasterTilesGeneratedNotification(notifyParams));
      }
      else
      {
        log.LogInformation($"Tile generation with Pegasus disabled (Bug 83657) - ignoring request: {JsonConvert.SerializeObject(request)}");
      }
    }

    public Task TearDown(object o, object context) => Task.FromResult(true);

    protected abstract Task<TileMetadata> GenerateTiles(TileGenerationRequest request);

    protected IHeaderDictionary CustomHeaders() =>
      new HeaderDictionary
      {
        {"Content-Type", ContentTypeConstants.ApplicationJson},
        {"Authorization", $"Bearer {authentication.GetApplicationBearerToken()}"}
      };

    protected void SetJobValues(IHeaderDictionary setJobIdAction)
    {
      if (jobContext == null) { return; }

      foreach (var item in setJobIdAction)
      {
        JobStorage.Current.GetConnection().SetJobParameter(jobContext.BackgroundJob.Id, item.Key, item.Value);
      }
    }
  }
}

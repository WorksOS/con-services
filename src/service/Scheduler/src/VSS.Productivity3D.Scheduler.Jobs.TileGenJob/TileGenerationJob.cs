using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Pegasus.Client;
using VSS.Pegasus.Client.Models;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.Jobs.DxfTileJob
{
  public abstract class TileGenerationJob<T> : IJob where T : TileGenerationRequest
  {
    public virtual Guid VSSJobUid { get; }

    protected readonly IPegasusClient pegasusClient;
    private readonly ITPaaSApplicationAuthentication authentication;
    private readonly INotificationHubClient notificationHubClient;
    protected readonly ILogger log;
    private readonly IConfigurationStore configStore;

    public TileGenerationJob(IConfigurationStore configurationStore, IPegasusClient pegasusClient, ITPaaSApplicationAuthentication authn, INotificationHubClient notificationHubClient, ILoggerFactory logger)
    {
      configStore = configurationStore;
      this.pegasusClient = pegasusClient;
      authentication = authn;
      this.notificationHubClient = notificationHubClient;
      log = logger.CreateLogger<DxfTileGenerationJob>();
    }

    public Task Setup(object o) => Task.FromResult(true);

    public async Task Run(object o)
    {
      T request;
      try
      {
        request = (o as JObject).ToObject<T>();
      }
      catch (Exception e)
      {
        log.LogError(e, "Exception when converting parameters to tile generation request");
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "Missing or Wrong parameters passed to tile generation job"));
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

    public Task TearDown(object o) => Task.FromResult(true);

    protected abstract Task<TileMetadata> GenerateTiles(TileGenerationRequest request);
   

    protected Dictionary<string, string> CustomHeaders() =>
      new Dictionary<string, string>
      {
        {"Content-Type", ContentTypeConstants.ApplicationJson},
        {"Authorization", $"Bearer {authentication.GetApplicationBearerToken()}"}
      };
  }
}

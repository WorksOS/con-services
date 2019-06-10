using System;
using System.Collections.Generic;
using System.IO;
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
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.Jobs.DxfTileJob
{
  /// <summary>
  /// Job to generate DXF tiles using Pegasus.
  /// </summary>
  public class DxfTileGenerationJob : IJob
  {
    public static Guid VSSJOB_UID = Guid.Parse("5f3eed28-58e8-451e-8459-5f5a39d5c3b6");
    public Guid VSSJobUid => VSSJOB_UID;

    private readonly IPegasusClient pegasusClient;
    private readonly ITPaaSApplicationAuthentication authentication;
    private readonly INotificationHubClient notificationHubClient;
    private readonly ILogger log;
    private readonly IConfigurationStore configStore;

    public DxfTileGenerationJob(IConfigurationStore configurationStore, IPegasusClient pegasusClient, ITPaaSApplicationAuthentication authn, INotificationHubClient notificationHubClient, ILoggerFactory logger)
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
      DxfTileGenerationRequest request;
      try
      {
        request = (o as JObject).ToObject<DxfTileGenerationRequest>();
      }
      catch (Exception e)
      {
        log.LogError(e, "Exception when converting parameters to DxfTileGenerationRequest");
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "Missing or Wrong parameters passed to DXF tile generation job"));
      }
      //Validate the parameters
      request.Validate();

      if (configStore.GetValueBool("SCHEDULER_ENABLE_DXF_TILE_GENERATION").HasValue &&
          configStore.GetValueBool("SCHEDULER_ENABLE_DXF_TILE_GENERATION").Value)
      {
        log.LogInformation($"DXF tile deneration with Pegasus enabled: {JsonConvert.SerializeObject(request)}");

        var dataOceanPath = $"{Path.DirectorySeparatorChar}{request.DataOceanRootFolder}{Path.DirectorySeparatorChar}{request.CustomerUid}{Path.DirectorySeparatorChar}{request.ProjectUid}";
        var dxfFileName = $"{dataOceanPath}{Path.DirectorySeparatorChar}{request.DxfFileName}";
        var dcFileName = $"{dataOceanPath}{Path.DirectorySeparatorChar}{request.DcFileName}";
        
        var result = await pegasusClient.GenerateDxfTiles(dcFileName, dxfFileName, request.DxfUnitsType, CustomHeaders());
        
        log.LogInformation($"Received Pegasus response for tile generation, filename: {dxfFileName}, result: `{JsonConvert.SerializeObject(result)}`");
        
        var notifyParams = new DxfTileNotificationParameters
        {
          FileUid = request.ImportedFileUid,
          MinZoomLevel = result.MinZoom,
          MaxZoomLevel = result.MaxZoom
        };

        await notificationHubClient.Notify(new ProjectFileDxfTilesGeneratedNotification(notifyParams));
      }
      else
      {
        log.LogInformation($"DXF tile generation with Pegasus disabled (Bug 83657) - ignoring request: {JsonConvert.SerializeObject(request)}");
      }
    }

    public Task TearDown(object o) => Task.FromResult(true);

    private Dictionary<string, string> CustomHeaders() =>
      new Dictionary<string, string>
      {
        {"Content-Type", ContentTypeConstants.ApplicationJson},
        {"Authorization", $"Bearer {authentication.GetApplicationBearerToken()}"}
      };
  }
}

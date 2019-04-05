using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Pegasus.Client;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.WebApi.Common;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;

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

    public DxfTileGenerationJob(IPegasusClient pegasusClient, ITPaaSApplicationAuthentication authn, INotificationHubClient notificationHubClient, ILoggerFactory logger)
    {
      this.pegasusClient = pegasusClient;
      authentication = authn;
      this.notificationHubClient = notificationHubClient;
      log = logger.CreateLogger<DxfTileGenerationJob>();
    }

    public Task Setup(object o)
    {
      return Task.FromResult(true);
    }

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

      var dataOceanPath = $"{Path.DirectorySeparatorChar}{request.DataOceanRootFolder}{Path.DirectorySeparatorChar}{request.CustomerUid}{Path.DirectorySeparatorChar}{request.ProjectUid}";
      var dxfFileName = $"{dataOceanPath}{Path.DirectorySeparatorChar}{request.DxfFileName}";
      var dcFileName = $"{dataOceanPath}{Path.DirectorySeparatorChar}{request.DcFileName}";

      var result = await pegasusClient.GenerateDxfTiles(dcFileName, dxfFileName, request.DxfUnitsType, CustomHeaders());
      var notifyParams = new DxfTileNotificationParameters
      {
        FileUid = request.ImportedFileUid,
        MinZoomLevel = result.MinZoom,
        MaxZoomLevel = result.MaxZoom
      };
      await notificationHubClient.Notify(new ProjectFileDxfTilesGeneratedNotification(notifyParams));
    }

    public Task TearDown(object o)
    {
      return Task.FromResult(true);
    }

    private Dictionary<string, string> CustomHeaders()
    {
      return new Dictionary<string, string>
      {
        {"Content-Type", ContentTypeConstants.ApplicationJson},
        {"Authorization", $"Bearer {authentication.GetApplicationBearerToken()}"}
      };
    }
  }
}

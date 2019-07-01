using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Pegasus.Client;
using VSS.Pegasus.Client.Models;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.Jobs.DxfTileJob
{
  /// <summary>
  /// Job to generate DXF tiles using Pegasus.
  /// </summary>
  public class DxfTileGenerationJob : TileGenerationJob<DxfTileGenerationRequest>
  {
    public static Guid VSSJOB_UID = Guid.Parse("5f3eed28-58e8-451e-8459-5f5a39d5c3b6");
    public override Guid VSSJobUid => VSSJOB_UID;

    public DxfTileGenerationJob(IConfigurationStore configurationStore, IPegasusClient pegasusClient, ITPaaSApplicationAuthentication authn, INotificationHubClient notificationHubClient, ILoggerFactory logger)
      : base(configurationStore, pegasusClient, authn, notificationHubClient, logger)
    {
      
    }

    protected override Task<TileMetadata> GenerateTiles(TileGenerationRequest request)
    {
      var dxfRequest = request as DxfTileGenerationRequest;
      if (dxfRequest == null)
      {
        log.LogError("Exception when converting parameters to DxfTileGenerationRequest");
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            "Missing or Wrong parameters passed to DXF tile generation job"));
      }
      dxfRequest.Validate();

      var dxfFileName = $"{dxfRequest.DataOceanPath}{Path.DirectorySeparatorChar}{dxfRequest.FileName}";
      var dcFileName = $"{dxfRequest.DataOceanPath}{Path.DirectorySeparatorChar}{dxfRequest.DcFileName}";

      return pegasusClient.GenerateDxfTiles(dcFileName, dxfFileName, dxfRequest.DxfUnitsType, CustomHeaders());
    }


  }
}

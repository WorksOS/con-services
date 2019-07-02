using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Pegasus.Client;
using VSS.Pegasus.Client.Models;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.Jobs.DxfTileJob
{
  /// <summary>
  /// Job to generate GeoTIFF tiles using Pegasus.
  /// </summary>
  public class GeoTiffTileGenerationJob : TileGenerationJob<TileGenerationRequest>
  {
    public static Guid VSSJOB_UID = Guid.Parse("2d9f4eb2-1991-49c7-a1b0-87e75c770cc1");
    public override Guid VSSJobUid => VSSJOB_UID;


    public GeoTiffTileGenerationJob(IConfigurationStore configurationStore, IPegasusClient pegasusClient, ITPaaSApplicationAuthentication authn, INotificationHubClient notificationHubClient, ILoggerFactory logger)
    : base(configurationStore, pegasusClient, authn, notificationHubClient, logger)
    {
     
    }

    protected override Task<TileMetadata> GenerateTiles(TileGenerationRequest request)
    {
      var geoTiffFileName = $"{request.DataOceanPath}{Path.DirectorySeparatorChar}{request.FileName}";

      return pegasusClient.GenerateGeoTiffTiles(geoTiffFileName, CustomHeaders());
    }

  }
}

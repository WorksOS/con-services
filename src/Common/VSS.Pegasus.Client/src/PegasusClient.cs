using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Pegasus.Client.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Pegasus.Client
{
  /// <summary>
  /// This is a client which is used to send requests to the Pegasus API. It uses GracefulWebRequest which uses HttpClient.
  /// </summary>
  public class PegasusClient : IPegasusClient
  {
    private const string PEGASUS_URL_KEY = "PEGASUS_URL";
    private const string PEGASUS_EXECUTION_TIMEOUT_KEY = "DATA_OCEAN_UPLOAD_TIMEOUT_MINS";
    private const string PEGASUS_EXECUTION_WAIT_KEY = "DATA_OCEAN_UPLOAD_WAIT_MILLSECS";

    private readonly Guid DXF_PROCEDURE_ID = new Guid("b8431158-1917-4d18-9f2e-e26b255900b7");
    private const string TILE_TYPE = "xyz";
    private const string TILE_ORDER = "YX";

    private readonly ILogger<PegasusClient> Log;
    private readonly IWebRequest gracefulClient;
    private readonly IDataOceanClient dataOceanClient;
    private readonly string pegasusBaseUrl;
    private readonly int executionWaitInterval;
    private readonly int executionTimeout;
    private readonly int maxZoomLevel;
    /// <summary>
    /// Client for sending requests to the Pegasus API.
    /// </summary>
    public PegasusClient(IConfigurationStore configuration, ILoggerFactory logger, IWebRequest gracefulClient, IDataOceanClient dataOceanClient)
    {
      Log = logger.CreateLogger<PegasusClient>();
      this.gracefulClient = gracefulClient;
      this.dataOceanClient = dataOceanClient;

      pegasusBaseUrl = configuration.GetValueString(PEGASUS_URL_KEY);
      if (string.IsNullOrEmpty(pegasusBaseUrl))
      {
        throw new ArgumentException($"Missing environment variable {PEGASUS_URL_KEY}");
      }
      Log.LogInformation($"{PEGASUS_URL_KEY}={pegasusBaseUrl}");
      executionWaitInterval = configuration.GetValueInt(PEGASUS_EXECUTION_WAIT_KEY, 1000);//Millisecs
      executionTimeout = configuration.GetValueInt(PEGASUS_EXECUTION_TIMEOUT_KEY, 5);//minutes
      maxZoomLevel = configuration.GetValueInt("TILE_RENDER_MAX_ZOOM_LEVEL", 21);
    }

    /// <summary>
    /// Generates DXF tiles using the Pegasus API and stores them in the data ocean.
    /// </summary>
    /// <param name="dcFileName">The path and file name of the coordinate system file</param>
    /// <param name="dxfFileName">The path and file name of the DXF file</param>
    /// <param name="dxfUnitsType">The units of the DXF file</param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<TileMetadata> GenerateDxfTiles(string dcFileName, string dxfFileName, DxfUnitsType dxfUnitsType, IDictionary<string, string> customHeaders)
    {
      Log.LogInformation($"GenerateDxfTiles: dcFileName={dcFileName}, dxfFileName={dxfFileName}, dxfUnitsType={dxfUnitsType}");

      TileMetadata metadata = null;
      //Get the DataOcean file ids.
      var dcFileId = await dataOceanClient.GetFileId(dcFileName, customHeaders);
      if (dcFileId == null)
      {
        Log.LogError($"Failed to find coordinate system file {dcFileName}. Has it been uploaded successfully?");
        return null;
      }
      var dxfFileId = await dataOceanClient.GetFileId(dxfFileName, customHeaders);
      if (dxfFileId == null)
      {
        Log.LogError($"Failed to find DXF file {dxfFileName}. Has it been uploaded successfully?");
        return null;
      }

      //Create the top level tiles folder
      string tileFolder = new DataOceanFileUtil(dxfFileName).GeneratedTilesFolder;
      var success = await dataOceanClient.MakeFolder(tileFolder, customHeaders);
      if (!success)
      {
        Log.LogError($"GenerateDxfTiles: Failed to create tiles folder {tileFolder}");
        return null;
      }
      var parentId = await dataOceanClient.GetFolderId(tileFolder, customHeaders);

      //1. Create an execution
      var units = dxfUnitsType.ToString();
      var createExecutionMessage = new CreateExecutionMessage
      {
        Execution = new PegasusExecution
        {
          ProcedureId = DXF_PROCEDURE_ID,
          Parameters = new PegasusExecutionParameters
          {
            DcFileId = dcFileId.Value,
            DxfFileId = dxfFileId.Value,
            ParentId = parentId,
            MaxZoom = maxZoomLevel,
            TileType = TILE_TYPE,
            TileOrder = TILE_ORDER,
            MultiFile = true,
            Public = false,
            Name = tileFolder,
            AngularUnit = units,
            PlaneUnit = units,
            VerticalUnit = units
          }
        }
      };
      const string baseRoute = "/api/executions";
      var payload = JsonConvert.SerializeObject(createExecutionMessage);
      PegasusExecution execution = null;
      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        execution = await gracefulClient.ExecuteRequest<PegasusExecution>($"{pegasusBaseUrl}{baseRoute}", ms, customHeaders, HttpMethod.Post, null, 3, false);
      }
      if (execution == null)
      {
        Log.LogError($"GenerateDxfTiles: Failed to create execution for {dxfFileName}");
        return null;
      }

      //2. Start the execution
      Log.LogDebug($"Starting execution for {dxfFileName}");
      var executionRoute = $"{baseRoute}/{execution.Id}";
      var startExecutionRoute = $"{executionRoute}/start";
      var result = await gracefulClient.ExecuteRequest<PegasusExecutionResult>($"{pegasusBaseUrl}{startExecutionRoute}", null, customHeaders, HttpMethod.Post, null, 3, false);
      if (result == null)
      {
        Log.LogError($"GenerateDxfTiles: Failed to start execution for {dxfFileName}");
        return null;
      }
      //3. Monitor status of execution until done
      Log.LogDebug($"Monitoring execution status for {dxfFileName}");

      DateTime endJob = DateTime.Now + TimeSpan.FromMinutes(executionTimeout);
      bool done = false;
      while (!done && DateTime.Now <= endJob)
      {
        if (executionWaitInterval > 0) await Task.Delay(executionWaitInterval);
        execution = await gracefulClient.ExecuteRequest<PegasusExecution>(executionRoute, null, customHeaders, HttpMethod.Get, null, 3, false);
        success = execution.ExecutionStatus == ExecutionStatus.FINISHED;
        done = success || execution.ExecutionStatus == ExecutionStatus.FAILED;
        Log.LogDebug($"Execution status {execution.ExecutionStatus} for {dxfFileName}");
      }

      if (!done)
      {
        Log.LogInformation($"GenerateDxfTiles timed out: {dxfFileName}");
      }
      else if (!success)
      {
        Log.LogInformation($"GenerateDxfTiles failed: {dxfFileName}");
      }

      if (success)
      {
        //4. Get the zoom range from the tile metdata file and publish notification
        Log.LogDebug($"Getting tiles metadata for {dxfFileName}");
        var metadataFileName = new DataOceanFileUtil(dxfFileName).TilesMetadataFileName;
        var stream = await dataOceanClient.GetFile(metadataFileName, customHeaders);

        using (var sr = new StreamReader(stream))
        using (var jtr = new JsonTextReader(sr))
        {
          var js = new JsonSerializer();
          metadata = js.Deserialize<TileMetadata>(jtr);
        }
      }

      Log.LogInformation($"GenerateDxfTiles: returning {(metadata == null ? "null" : JsonConvert.SerializeObject(metadata))}");
      return metadata;
    }

  }
}

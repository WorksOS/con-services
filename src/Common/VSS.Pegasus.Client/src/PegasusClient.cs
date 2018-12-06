using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.Pegasus.Client.Models;

namespace VSS.Pegasus.Client
{
  /// <summary>
  /// This is a client which is used to send requests to the Pegasus API. It uses GracefulWebRequest which uses HttpClient.
  /// </summary>
  public class PegasusClient : IPegasusClient
  {
    private const string PEGASUS_URL_KEY = "DATA_OCEAN_URL";
    private Guid DXF_PROCEDURE_ID = new Guid("899c119c-3fd9-4feb-ac65-42cec9f4de08");
    private const string TILE_TYPE = "xyz";
    private const string TILE_ORDER = "yx";

    private readonly ILogger<PegasusClient> Log;
    private readonly ILoggerFactory logFactory;
    private readonly IConfigurationStore configStore;
    private readonly IWebRequest gracefulClient;
    private readonly IDataOceanClient dataOceanClient;
    private readonly string pegasusBaseUrl;
    private readonly int maxZoomLevel;
    /// <summary>
    /// Client for sending requests to the pegasus API.
    /// </summary>
    public PegasusClient(IConfigurationStore configuration, ILoggerFactory logger, IWebRequest gracefulClient, IDataOceanClient dataOceanClient)
    {
      logFactory = logger;
      Log = logger.CreateLogger<PegasusClient>();
      configStore = configuration;
      this.gracefulClient = gracefulClient;
      this.dataOceanClient = dataOceanClient;

      pegasusBaseUrl = configuration.GetValueString(PEGASUS_URL_KEY);
      if (string.IsNullOrEmpty(pegasusBaseUrl))
      {
        throw new Exception($"Missing environment variable {PEGASUS_URL_KEY}");
      }
      Log.LogInformation($"{PEGASUS_URL_KEY}={pegasusBaseUrl}");
      maxZoomLevel = configuration.GetValueInt("TILE_RENDER_MAX_ZOOM_LEVEL", 21);
    }

    /// <summary>
    /// Generates DXF tiles and stores them in the data ocean.
    /// </summary>
    /// <param name="dcFileName">The path and file name of the coordinate system file</param>
    /// <param name="dxfFileName">The path and file name of the DXF file</param>
    /// <returns></returns>
    public async Task<PegasusExecution> GenerateDxfTiles(string dcFileName, string dxfFileName, IDictionary<string, string> customHeaders)
    {
      //Get the DataOcean file ids.
      var dcFile = await dataOceanClient.GetFile();
      var dxfFile = await dataOceanClient.GetFile();
      //TODO: if fails to get files throw exception

      //Create the top level tiles folder
      //TODO: generate the name as per 3dpm
      string tileFolder = "";
      var success = dataOceanClient.MakeFolder();

      //TODO: dataocean client will need to expose the id for the parent_id for create execution
      Guid parentId = null;

      //Create an execution
      var createExecutionMessage = new CreateExecutionMessage
      {
        Execution = new PegasusExecution
        {
          ProcedureId = DXF_PROCEDURE_ID,
          Parameters = new PegasusExecutionParameters
          {
            DcFileId = dcFile.Id,
            DxfFileId = dxfFile.Id,
            ParentId = parentId,
            MaxZoom = maxZoomLevel,
            TileType = TILE_TYPE,
            TileOrder = TILE_ORDER,
            MultiFile = true,
            Public = false,
            Name = tileFolder
          }
        }
      };
      //TODO: return type
      var createResult = await CreateExecution<bool>(createExecutionMessage, "/api/executions", customHeaders);

      //Start the execution

      //Monitor its status

    }

    /// <summary>
    /// Creates a Pegasus execution.
    /// </summary>
    /// <typeparam name="T">The type of data returned</typeparam>
    /// <param name="message">The message payload</param>
    /// <param name="route">The route for the request</param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    private async Task<T> CreateExecution<T>(CreateExecutionMessage message, string route, IDictionary<string, string> customHeaders)
    {
      var payload = JsonConvert.SerializeObject(message);
      Log.LogDebug($"CreateExecution: route={route}, message={payload}");

      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        var result = await gracefulClient.ExecuteRequest<T>($"{pegasusBaseUrl}{route}", ms, customHeaders, HttpMethod.Post, null, 3, false);
        Log.LogDebug($"CreateExecution: result={JsonConvert.SerializeObject(result)}");
        return result;
      }
    }
  }
}

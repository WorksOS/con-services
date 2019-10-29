using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.ResultHandling.Abstractions;
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
    private const string PEGASUS_EXECUTION_TIMEOUT_MINS = "PEGASUS_EXECUTION_TIMEOUT_MINS";
    private const string PEGASUS_EXECUTION_WAIT_MILLSECS = "PEGASUS_EXECUTION_WAIT_MILLSECS";
    private const string PEGASUS_DXF_PROCEDURE_ID = "PEGASUS_DXF_PROCEDURE_ID";
    private const string PEGASUS_GEOTIFF_PROCEDURE_ID = "PEGASUS_GEOTIFF_PROCEDURE_ID";
    private const string PEGASUS_LOG_JOBID_KEY = "pegasus_jobid";
    private const string PEGASUS_LOG_RESULT_KEY = "pegasus_result";
    private const string PEGASUS_LOG_EVENTS_KEY = "pegasus_events";

    private const string TILE_TYPE = "xyz";
    private const string TILE_ORDER = "YX";
    private const string TILE_EXPORT_FORMAT = "xyz";
    private const string TILE_OUTPUT_FORMAT = "PNGRASTER";
    private const string TILE_CRS = "EPSG:3857";

    private readonly ILogger<PegasusClient> Log;
    private readonly IWebRequest gracefulClient;
    private readonly IDataOceanClient dataOceanClient;
    private readonly string pegasusBaseUrl;
    private readonly int executionWaitInterval;
    private readonly double executionTimeout;
    private readonly int maxZoomLevel;
    private readonly Guid dxfProcedureId;
    private readonly Guid geoTiffProcedureId;

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
      executionWaitInterval = configuration.GetValueInt(PEGASUS_EXECUTION_WAIT_MILLSECS, 1000);
      executionTimeout = configuration.GetValueDouble(PEGASUS_EXECUTION_TIMEOUT_MINS, 5);
      maxZoomLevel = configuration.GetValueInt("TILE_RENDER_MAX_ZOOM_LEVEL", 21);
      dxfProcedureId = configuration.GetValueGuid(PEGASUS_DXF_PROCEDURE_ID);
      if (dxfProcedureId == Guid.Empty)
      {
        throw new ArgumentException($"Missing environment variable {PEGASUS_DXF_PROCEDURE_ID}");
      }
      geoTiffProcedureId = configuration.GetValueGuid(PEGASUS_GEOTIFF_PROCEDURE_ID);
      if (geoTiffProcedureId == Guid.Empty)
      {
        throw new ArgumentException($"Missing environment variable {PEGASUS_GEOTIFF_PROCEDURE_ID}");
      }
    }

    /// <summary>
    /// Generates DXF tiles using the Pegasus API and stores them in the data ocean.
    /// </summary>
    /// <param name="dcFileName">The path and file name of the coordinate system file</param>
    /// <param name="dxfFileName">The path and file name of the DXF file</param>
    /// <param name="dxfUnitsType">The units of the DXF file</param>
    /// <param name="customHeaders"></param>
    /// <param name="setJobIdAction"></param>
    /// <returns>Metadata for the generated tiles including the zoom range</returns>
    public async Task<TileMetadata> GenerateDxfTiles(string dcFileName, string dxfFileName, DxfUnitsType dxfUnitsType, IDictionary<string, string> customHeaders, Action<IDictionary<string, string>> setJobIdAction)
    {
      Log.LogInformation($"{nameof(GenerateDxfTiles)}: dcFileName={dcFileName}, dxfFileName={dxfFileName}, dxfUnitsType={dxfUnitsType}");

      //Get the DataOcean file ids.
      var dcFileId = await dataOceanClient.GetFileId(dcFileName, customHeaders);
      if (dcFileId == null)
      {
        var message = $"Failed to find coordinate system file {dcFileName}. Has it been uploaded successfully?";
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, message));
      }
      var dxfFileId = await dataOceanClient.GetFileId(dxfFileName, customHeaders);
      if (dxfFileId == null)
      {
        var message = $"Failed to find DXF file {dxfFileName}. Has it been uploaded successfully?";
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, message));
      }

      //Get the Pegasus units
      var pegasusUnits = PegasusUnitsType.Metre;
      switch (dxfUnitsType)
      {
        case DxfUnitsType.Meters:
          break;
        case DxfUnitsType.UsSurveyFeet:
          pegasusUnits = PegasusUnitsType.USSurveyFoot;
          break;
        case DxfUnitsType.ImperialFeet:
          pegasusUnits = PegasusUnitsType.BritishFoot;
          break;
      }

      //1. Create an execution
      var createExecutionMessage = new CreateExecutionMessage
      {
        Execution = new PegasusExecution
        {
          ProcedureId = dxfProcedureId,
          Parameters = new DxfPegasusExecutionParameters
          {
            DcFileId = dcFileId.Value,
            DxfFileId = dxfFileId.Value,
            MaxZoom = maxZoomLevel.ToString(),
            TileType = TILE_TYPE,
            AngularUnit = AngularUnitsType.Degree.ToString(),
            PlaneUnit = pegasusUnits.ToString(),
            VerticalUnit = pegasusUnits.ToString()
          }
        }
      };

      return await GenerateTiles(dxfFileName, createExecutionMessage, customHeaders, setJobIdAction);
    }


    /// <summary>
    /// Generates GeoTIFF tiles using the Pegasus API and stores them in the data ocean.
    /// </summary>
    /// <param name="geoTiffFileName">The path and file name of the GeoTIFF file</param>
    /// <param name="customHeaders"></param>
    /// <returns>Metadata for the generated tiles including the zoom range</returns>
    public async Task<TileMetadata> GenerateGeoTiffTiles(string geoTiffFileName, IDictionary<string, string> customHeaders, Action<IDictionary<string, string>> setJobIdAction)
    {
      Log.LogInformation($"{nameof(GenerateGeoTiffTiles)}: geoTiffFileName={geoTiffFileName}");

      //Get the DataOcean file id.
      var geoTiffFileId = await dataOceanClient.GetFileId(geoTiffFileName, customHeaders);
      if (geoTiffFileId == null)
      {
        var message = $"Failed to find GeoTIFF file {geoTiffFileName}. Has it been uploaded successfully?";
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, message));
      }

      //1. Create an execution
      var createExecutionMessage = new CreateExecutionMessage
      {
        Execution = new PegasusExecution
        {
          ProcedureId = geoTiffProcedureId,
          Parameters = new GeoTiffPegasusExecutionParameters
          {
            GeoTiffFileId = geoTiffFileId.Value,
            TileExportFormat = TILE_EXPORT_FORMAT,
            TileOutputFormat = TILE_OUTPUT_FORMAT,
            TileCrs = TILE_CRS
          }
        }
      };

      return await GenerateTiles(geoTiffFileName, createExecutionMessage, customHeaders, setJobIdAction);
    }

    /// <summary>
    /// Generates raster tiles using the Pegasus API and stores them in the data ocean.
    /// The source is either a DXF file or a GeoTIFF file.
    /// </summary>
    /// <param name="fileName">The path and file name of the source file</param>
    /// <param name="createExecutionMessage">The details of tile generation for Pegasus</param>
    /// <param name="customHeaders"></param>
    /// <returns>Metadata for the generated tiles including the zoom range</returns>
    private async Task<TileMetadata> GenerateTiles(string fileName, CreateExecutionMessage createExecutionMessage, IDictionary<string, string> customHeaders, Action<IDictionary<string, string>> setJobIdAction)
    {
      TileMetadata metadata = null;

      //Delete any old tiles. To avoid 2 traversals just try the delete anyway without checking for existance.
      await DeleteTiles(fileName, customHeaders);

      //In DataOcean this is actually a multifile not a folder
      string tileFolderFullName = new DataOceanFileUtil(fileName).GeneratedTilesFolder;
      //Get the parent folder id
      var parts = tileFolderFullName.Split(DataOceanUtil.PathSeparator);
      var tileFolderName = parts[parts.Length - 1];
      var parentPath = tileFolderFullName.Substring(0, tileFolderFullName.Length - tileFolderName.Length - 1);
      var parentId = await dataOceanClient.GetFolderId(parentPath, customHeaders);
      //Set common parameters
      createExecutionMessage.Execution.Parameters.ParentId = parentId;
      createExecutionMessage.Execution.Parameters.Name = tileFolderName;
      createExecutionMessage.Execution.Parameters.TileOrder = TILE_ORDER;
      createExecutionMessage.Execution.Parameters.MultiFile = "true";
      createExecutionMessage.Execution.Parameters.Public = "false";

      const string baseRoute = "/api/executions";
      var payload = JsonConvert.SerializeObject(createExecutionMessage);
      PegasusExecutionResult executionResult;

      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        executionResult = await gracefulClient.ExecuteRequest<PegasusExecutionResult>($"{pegasusBaseUrl}{baseRoute}", ms, customHeaders, HttpMethod.Post);
      }

      if (executionResult == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Failed to create execution for {fileName}"));
      }

      setJobIdAction?.Invoke(new Dictionary<string, string> { { PEGASUS_LOG_JOBID_KEY, executionResult.Execution.Id.ToString() } });

      //2. Start the execution
      Log.LogDebug($"Starting execution for {fileName}");
      var executionRoute = $"{baseRoute}/{executionResult.Execution.Id}";
      var startExecutionRoute = $"{executionRoute}/start";
      var startResult = await gracefulClient.ExecuteRequest<PegasusExecutionAttemptResult>($"{pegasusBaseUrl}{startExecutionRoute}", null, customHeaders, HttpMethod.Post);
      if (startResult == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Failed to start execution for {fileName}"));
      }

      //3. Monitor status of execution until done
      Log.LogDebug($"Monitoring execution status for {fileName}");

      var endJob = DateTime.Now + TimeSpan.FromMinutes(executionTimeout);
      var done = false;
      var success = true;

      while (!done && DateTime.Now <= endJob)
      {
        if (executionWaitInterval > 0) { await Task.Delay(executionWaitInterval); }

        var policyResult = await Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
          3,
          attempt => TimeSpan.FromMilliseconds(1000),
          (exception, calculatedWaitDuration) =>
          {
            Log.LogError(exception, $"PollyAsync: Failed attempt to query Pegasus. Jobid {executionResult.Execution.Id.ToString()}");
          })
        .ExecuteAndCaptureAsync(async () =>
        {
          Log.LogDebug($"Executing monitoring request for {fileName} and jobid {executionResult.Execution.Id.ToString()}");
          executionResult = await gracefulClient.ExecuteRequest<PegasusExecutionResult>($"{pegasusBaseUrl}{executionRoute}", null, customHeaders, HttpMethod.Get);
          var status = executionResult.Execution.ExecutionStatus;
          success = string.Compare(status, ExecutionStatus.FINISHED, StringComparison.OrdinalIgnoreCase) == 0 ||
                    string.Compare(status, ExecutionStatus.SUCCEEDED, StringComparison.OrdinalIgnoreCase) == 0;

          if (string.Compare(status, ExecutionStatus.FAILED, StringComparison.OrdinalIgnoreCase) == 0)
          {
            //Try to retrieve why it failed
            var jobEventsStream = await gracefulClient.ExecuteRequestAsStreamContent($"{pegasusBaseUrl}{executionRoute}/events", HttpMethod.Get, customHeaders);

            if (jobEventsStream != null)
            {
              var jobEvents = await jobEventsStream.ReadAsStringAsync();
              Log.LogError($"Pegasus job {executionResult.Execution.Id.ToString()} failed to execute with the events: {jobEvents}");
              setJobIdAction?.Invoke(new Dictionary<string, string> { { PEGASUS_LOG_EVENTS_KEY, jobEvents } });
            }
            else
            {
              Log.LogDebug($"Unable to resolve jobEventsStream for execution id {executionResult.Execution.Id}");
            }
          }

          done = success || string.Compare(status, ExecutionStatus.FAILED, StringComparison.OrdinalIgnoreCase) == 0;

          setJobIdAction?.Invoke(new Dictionary<string, string> { { PEGASUS_LOG_RESULT_KEY, status } });

          Log.LogDebug($"Execution status {status} for {fileName} and jobid {executionResult.Execution.Id.ToString()}");
        });

        if (policyResult.FinalException != null)
        {
          Log.LogCritical(policyResult.FinalException,
            $"TileGeneration PollyAsync: {GetType().FullName} failed with exception for jobid {executionResult.Execution.Id.ToString()}: ");
          throw policyResult.FinalException;
        }
      }

      if (!done)
      {
        Log.LogInformation($"{nameof(GenerateTiles)} timed out: {fileName}");
      }
      else if (!success)
      {
        Log.LogInformation($"{nameof(GenerateTiles)} failed: {fileName}");
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Failed to generate tiles for {fileName}"));
      }

      if (success)
      {
        /*
         Can't delete as not mutable

        //4. Delete the execution
        Log.LogDebug($"Deleting execution for {dxfFileName}");
        await gracefulClient.ExecuteRequest($"{pegasusBaseUrl}{executionRoute}", null, customHeaders, HttpMethod.Delete, null, 3, false);
        */

        //5. Get the zoom range from the tile metadata file 
        var metadataFileName = new DataOceanFileUtil(fileName).TilesMetadataFileName;
        Log.LogDebug($"Getting tiles metadata for {metadataFileName}");
        var stream = await dataOceanClient.GetFile(metadataFileName, customHeaders);

        using (var sr = new StreamReader(stream))
        using (var jtr = new JsonTextReader(sr))
        {
          metadata = new JsonSerializer().Deserialize<TileMetadata>(jtr);
        }
      }

      Log.LogInformation($"{nameof(GenerateTiles)}: returning {(metadata == null ? "null" : JsonConvert.SerializeObject(metadata))}");

      return metadata;
    }

    /// <summary>
    /// Deletes generated tiles for the given file
    /// </summary>
    /// <param name="fileName">DXF or GeoTIFF file</param>
    /// <param name="customHeaders"></param>
    /// <returns>True if successfully deleted otherwise false</returns>
    public Task<bool> DeleteTiles(string fileName, IDictionary<string, string> customHeaders)
    {
      //In DataOcean this is actually a multifile not a folder
      string tileFolderFullName = new DataOceanFileUtil(fileName).GeneratedTilesFolder;
      //To avoid 2 traversals just try the delete anyway without checking for existance.
      return dataOceanClient.DeleteFile(tileFolderFullName, customHeaders);
    }
  }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
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
    private const string PEGASUS_EXECUTION_TIMEOUT_KEY = "PEGASUS_EXECUTION_TIMEOUT_MINS";
    private const string PEGASUS_EXECUTION_WAIT_KEY = "PEGASUS_EXECUTION_WAIT_MILLSECS";
    private const string PEGASUS_DXF_PROCEDURE_ID_KEY = "PEGASUS_DXF_PROCEDURE_ID";
    private const string PEGASUS_GEOTIFF_PROCEDURE_ID_KEY = "PEGASUS_GEOTIFF_PROCEDURE_ID";

    private const string TILE_TYPE = "xyz";
    private const string TILE_ORDER = "YX";

    private readonly ILogger<PegasusClient> Log;
    private readonly IWebRequest gracefulClient;
    private readonly IDataOceanClient dataOceanClient;
    private readonly string pegasusBaseUrl;
    private readonly int executionWaitInterval;
    private readonly int executionTimeout;
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
      executionWaitInterval = configuration.GetValueInt(PEGASUS_EXECUTION_WAIT_KEY, 1000);//Millisecs
      executionTimeout = configuration.GetValueInt(PEGASUS_EXECUTION_TIMEOUT_KEY, 5);//minutes
      maxZoomLevel = configuration.GetValueInt("TILE_RENDER_MAX_ZOOM_LEVEL", 21);
      if (!Guid.TryParse(configuration.GetValueString(PEGASUS_DXF_PROCEDURE_ID_KEY), out dxfProcedureId))
      {
        throw new ArgumentException($"Missing environment variable {PEGASUS_DXF_PROCEDURE_ID_KEY}");
      }
      if (!Guid.TryParse(configuration.GetValueString(PEGASUS_GEOTIFF_PROCEDURE_ID_KEY), out geoTiffProcedureId))
      {
        throw new ArgumentException($"Missing environment variable {PEGASUS_GEOTIFF_PROCEDURE_ID_KEY}");
      }
    }

    /// <summary>
    /// Generates DXF tiles using the Pegasus API and stores them in the data ocean.
    /// </summary>
    /// <param name="dcFileName">The path and file name of the coordinate system file</param>
    /// <param name="dxfFileName">The path and file name of the DXF file</param>
    /// <param name="dxfUnitsType">The units of the DXF file</param>
    /// <param name="customHeaders"></param>
    /// <returns>Metadata for the generated tiles including the zoom range</returns>
    public async Task<TileMetadata> GenerateDxfTiles(string dcFileName, string dxfFileName, DxfUnitsType dxfUnitsType, IDictionary<string, string> customHeaders)
    {
      Log.LogInformation($"{nameof(GenerateDxfTiles)}: dcFileName={dcFileName}, dxfFileName={dxfFileName}, dxfUnitsType={dxfUnitsType}");

      TileMetadata metadata = null;
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

      //Delete any old tiles. To avoid 2 traversals just try the delete anyway without checking for existance.
      await DeleteDxfTiles(dxfFileName, customHeaders);

      //In DataOcean this is actually a multifile not a folder
      string tileFolderFullName = new DataOceanFileUtil(dxfFileName).GeneratedTilesFolder;
      //Get the parent folder id
      var parts = tileFolderFullName.Split(Path.DirectorySeparatorChar);
      var tileFolderName = parts[parts.Length - 1];
      var parentPath = tileFolderFullName.Substring(0, tileFolderFullName.Length - tileFolderName.Length - 1);
      var parentId = await dataOceanClient.GetFolderId(parentPath, customHeaders);

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
            ParentId = parentId,
            MaxZoom = maxZoomLevel,
            TileType = TILE_TYPE,
            TileOrder = TILE_ORDER,
            MultiFile = "true",
            Public = "false",
            Name = tileFolderName,
            AngularUnit = AngularUnitsType.Degree.ToString(),
            PlaneUnit = pegasusUnits.ToString(),
            VerticalUnit = pegasusUnits.ToString()
          }
        }
      };
      const string baseRoute = "/api/executions";
      var payload = JsonConvert.SerializeObject(createExecutionMessage);
      PegasusExecutionResult executionResult = null;
      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        executionResult = await gracefulClient.ExecuteRequest<PegasusExecutionResult>($"{pegasusBaseUrl}{baseRoute}", ms, customHeaders, HttpMethod.Post, null, 3, false);
      }
      if (executionResult == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Failed to create execution for {dxfFileName}"));
      }

      //2. Start the execution
      Log.LogDebug($"Starting execution for {dxfFileName}");
      var executionRoute = $"{baseRoute}/{executionResult.Execution.Id}";
      var startExecutionRoute = $"{executionRoute}/start";
      var startResult = await gracefulClient.ExecuteRequest<PegasusExecutionAttemptResult>($"{pegasusBaseUrl}{startExecutionRoute}", null, customHeaders, HttpMethod.Post, null, 3, false);
      if (startResult == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Failed to start execution for {dxfFileName}"));
      }

      //3. Monitor status of execution until done
      Log.LogDebug($"Monitoring execution status for {dxfFileName}");
      DateTime endJob = DateTime.Now + TimeSpan.FromMinutes(executionTimeout);
      var done = false;
      var success = true;
      while (!done && DateTime.Now <= endJob)
      {
        if (executionWaitInterval > 0) await Task.Delay(executionWaitInterval);
        executionResult = await gracefulClient.ExecuteRequest<PegasusExecutionResult>($"{pegasusBaseUrl}{executionRoute}", null, customHeaders, HttpMethod.Get, null, 3, false);
        success = executionResult.Execution.ExecutionStatus == ExecutionStatus.FINISHED || executionResult.Execution.ExecutionStatus == ExecutionStatus.SUCCEEDED;
        done = success || executionResult.Execution.ExecutionStatus == ExecutionStatus.FAILED;
        Log.LogDebug($"Execution status {executionResult.Execution.ExecutionStatus} for {dxfFileName}");
      }

      if (!done)
      {
        Log.LogInformation($"{nameof(GenerateDxfTiles)} timed out: {dxfFileName}");
      }
      else if (!success)
      {
        Log.LogInformation($"{nameof(GenerateDxfTiles)} failed: {dxfFileName}");
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Failed to generate DXF tiles for {dxfFileName}"));
      }

      if (success)
      {
        /*
         Can't delete as not mutable

        //4. Delete the execution
        Log.LogDebug($"Deleting execution for {dxfFileName}");
        await gracefulClient.ExecuteRequest($"{pegasusBaseUrl}{executionRoute}", null, customHeaders, HttpMethod.Delete, null, 3, false);
        */

        //5. Get the zoom range from the tile metdata file 
        var metadataFileName = new DataOceanFileUtil(dxfFileName).TilesMetadataFileName;
        Log.LogDebug($"Getting tiles metadata for {metadataFileName}");
        var stream = await dataOceanClient.GetFile(metadataFileName, customHeaders);

        using (var sr = new StreamReader(stream))
        using (var jtr = new JsonTextReader(sr))
        {
          var js = new JsonSerializer();
          metadata = js.Deserialize<TileMetadata>(jtr);
        }
      }

      Log.LogInformation($"{nameof(GenerateDxfTiles)}: returning {(metadata == null ? "null" : JsonConvert.SerializeObject(metadata))}");
      return metadata;
    }

    /// <summary>
    /// Deletes the generated DXF tiles for the given DXF file
    /// </summary>
    /// <param name="dxfFileName">The path and file name of the DXF file</param>
    /// <param name="customHeaders"></param>
    /// <returns>True if successfully deleted otherwise false</returns>
    public async Task<bool> DeleteDxfTiles(string dxfFileName, IDictionary<string, string> customHeaders)
    {
      return await DeleteTiles(dxfFileName, customHeaders);
    }

    /// <summary>
    /// Generates GeoTIFF tiles using the Pegasus API and stores them in the data ocean.
    /// </summary>
    /// <param name="geoTiffFileName">The path and file name of the GeoTIFF file</param>
    /// <param name="customHeaders"></param>
    /// <returns>Metadata for the generated tiles including the zoom range</returns>
    public async Task<TileMetadata> GenerateGeoTiffTiles(string geoTiffFileName, IDictionary<string, string> customHeaders)
    {
      Log.LogInformation($"{nameof(GenerateGeoTiffTiles)}: geoTiffFileName={geoTiffFileName}");

      TileMetadata metadata = null;
      //Get the DataOcean file id.
      var geoTiffFileId = await dataOceanClient.GetFileId(geoTiffFileName, customHeaders);
      if (geoTiffFileId == null)
      {
        var message = $"Failed to find GeoTIFF file {geoTiffFileName}. Has it been uploaded successfully?";
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, message));
      }

      //Delete any old tiles. To avoid 2 traversals just try the delete anyway without checking for existance.
      await DeleteTiles(geoTiffFileName, customHeaders);

      //In DataOcean this is actually a multifile not a folder
      string tileFolderFullName = new DataOceanFileUtil(geoTiffFileName).GeneratedTilesFolder;
      //Get the parent folder id
      var parts = tileFolderFullName.Split(Path.DirectorySeparatorChar);
      var tileFolderName = parts[parts.Length - 1];
      var parentPath = tileFolderFullName.Substring(0, tileFolderFullName.Length - tileFolderName.Length - 1);
      var parentId = await dataOceanClient.GetFolderId(parentPath, customHeaders);

      //1. Create an execution
      var createExecutionMessage = new CreateExecutionMessage
      {
        Execution = new PegasusExecution
        {
          ProcedureId = geoTiffProcedureId,
          Parameters = new GeoTiffPegasusExecutionParameters
          {
            //DxfFileId = dxfFileId.Value,
            ParentId = parentId,
            //MaxZoom = maxZoomLevel,
            //TileType = TILE_TYPE,
            TileOrder = TILE_ORDER,
            MultiFile = "true",
            Public = "false",
            Name = tileFolderName,
            //AngularUnit = AngularUnitsType.Degree.ToString(),
            //PlaneUnit = pegasusUnits.ToString(),
            //VerticalUnit = pegasusUnits.ToString()
          }
        }
      };
      const string baseRoute = "/api/executions";
      var payload = JsonConvert.SerializeObject(createExecutionMessage);
      PegasusExecutionResult executionResult = null;
      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        executionResult = await gracefulClient.ExecuteRequest<PegasusExecutionResult>($"{pegasusBaseUrl}{baseRoute}", ms, customHeaders, HttpMethod.Post, null, 3, false);
      }
      if (executionResult == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Failed to create execution for {geoTiffFileName}"));
      }

      //2. Start the execution
      Log.LogDebug($"Starting execution for {geoTiffFileName}");
      var executionRoute = $"{baseRoute}/{executionResult.Execution.Id}";
      var startExecutionRoute = $"{executionRoute}/start";
      var startResult = await gracefulClient.ExecuteRequest<PegasusExecutionAttemptResult>($"{pegasusBaseUrl}{startExecutionRoute}", null, customHeaders, HttpMethod.Post, null, 3, false);
      if (startResult == null)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Failed to start execution for {geoTiffFileName}"));
      }

      //3. Monitor status of execution until done
      Log.LogDebug($"Monitoring execution status for {geoTiffFileName}");
      DateTime endJob = DateTime.Now + TimeSpan.FromMinutes(executionTimeout);
      var done = false;
      var success = true;
      while (!done && DateTime.Now <= endJob)
      {
        if (executionWaitInterval > 0) await Task.Delay(executionWaitInterval);
        executionResult = await gracefulClient.ExecuteRequest<PegasusExecutionResult>($"{pegasusBaseUrl}{executionRoute}", null, customHeaders, HttpMethod.Get, null, 3, false);
        success = executionResult.Execution.ExecutionStatus == ExecutionStatus.FINISHED || executionResult.Execution.ExecutionStatus == ExecutionStatus.SUCCEEDED;
        done = success || executionResult.Execution.ExecutionStatus == ExecutionStatus.FAILED;
        Log.LogDebug($"Execution status {executionResult.Execution.ExecutionStatus} for {geoTiffFileName}");
      }

      if (!done)
      {
        Log.LogInformation($"{nameof(GenerateGeoTiffTiles)} timed out: {geoTiffFileName}");
      }
      else if (!success)
      {
        Log.LogInformation($"{nameof(GenerateGeoTiffTiles)} failed: {geoTiffFileName}");
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Failed to generate DXF tiles for {geoTiffFileName}"));
      }

      if (success)
      {
        /*
         Can't delete as not mutable

        //4. Delete the execution
        Log.LogDebug($"Deleting execution for {dxfFileName}");
        await gracefulClient.ExecuteRequest($"{pegasusBaseUrl}{executionRoute}", null, customHeaders, HttpMethod.Delete, null, 3, false);
        */

        //5. Get the zoom range from the tile metdata file 
        var metadataFileName = new DataOceanFileUtil(geoTiffFileName).TilesMetadataFileName;
        Log.LogDebug($"Getting tiles metadata for {metadataFileName}");
        var stream = await dataOceanClient.GetFile(metadataFileName, customHeaders);

        using (var sr = new StreamReader(stream))
        using (var jtr = new JsonTextReader(sr))
        {
          var js = new JsonSerializer();
          metadata = js.Deserialize<TileMetadata>(jtr);
        }
      }

      Log.LogInformation($"{nameof(GenerateGeoTiffTiles)}: returning {(metadata == null ? "null" : JsonConvert.SerializeObject(metadata))}");
      return metadata;
    }


    /// <summary>
    /// Deletes the generated GeoTIFF tiles for the given GeoTIFF file
    /// </summary>
    /// <param name="geoTiffFileName">The path and file name of the GeoTIFF file</param>
    /// <param name="customHeaders"></param>
    /// <returns>True if successfully deleted otherwise false</returns>
    public async Task<bool> DeleteGeoTiffTiles(string geoTiffFileName, IDictionary<string, string> customHeaders)
    {
      return await DeleteTiles(geoTiffFileName, customHeaders);
    }

    /// <summary>
    /// Deletes generated tiles for the given file
    /// </summary>
    /// <param name="fileName">DXF or GeoTIFF file</param>
    /// <param name="customHeaders"></param>
    /// <returns>True if successfully deleted otherwise false</returns>
    private async Task<bool> DeleteTiles(string fileName, IDictionary<string, string> customHeaders)
    {
      //In DataOcean this is actually a multifile not a folder
      string tileFolderFullName = new DataOceanFileUtil(fileName).GeneratedTilesFolder;
      //To avoid 2 traversals just try the delete anyway without checking for existance.
      return await dataOceanClient.DeleteFile(tileFolderFullName, customHeaders);
    }

  }
}

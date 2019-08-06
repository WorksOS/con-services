using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// TerrainController responsible for all quantized mesh tile requests
  /// </summary>
  [Route("api/[controller]")]
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class TerrainController : ControllerBase, ITerrainContract
  {

    /// <summary>
    /// LoggerFactory for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore ConfigStore;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// The TRex Gateway proxy for use by executor.
    /// </summary>
 //   private readonly ITRexCompactionDataProxy TRexCompactionDataProxy;

    public const string layer = "{ \"tilejson\": \"2.1.0\",   \"name\": \"christchurch-15m-dem\",   \"description\": \"\",   \"version\": \"1.1.0\",   \"format\": \"quantized-mesh-1.0\",   \"attribution\": \"\",   \"schema\": \"tms\",   \"tiles\": [ \"{z}/{x}/{y}.terrain?v={version}\" ],   \"projection\": \"EPSG:4326\",   \"bounds\": [ 0.00, -90.00, 180.00, 90.00 ],   \"available\": [     [ { \"startX\": 0, \"startY\": 0, \"endX\": 1, \"endY\": 0 } ]    ,[ { \"startX\": 3, \"startY\": 0, \"endX\": 3, \"endY\": 0 } ]    ,[ { \"startX\": 7, \"startY\": 1, \"endX\": 7, \"endY\": 1 } ]    ,[ { \"startX\": 15, \"startY\": 2, \"endX\": 15, \"endY\": 2 } ]    ,[ { \"startX\": 31, \"startY\": 4, \"endX\": 31, \"endY\": 4 } ]    ,[ { \"startX\": 62, \"startY\": 8, \"endX\": 62, \"endY\": 8 } ]    ,[ { \"startX\": 125, \"startY\": 16, \"endX\": 125, \"endY\": 16 } ]    ,[ { \"startX\": 250, \"startY\": 32, \"endX\": 250, \"endY\": 33 } ]    ,[ { \"startX\": 501, \"startY\": 65, \"endX\": 501, \"endY\": 66 } ]    ,[ { \"startX\": 1002, \"startY\": 131, \"endX\": 1003, \"endY\": 132 } ]    ,[ { \"startX\": 2004, \"startY\": 263, \"endX\": 2007, \"endY\": 265 } ]    ,[ { \"startX\": 4009, \"startY\": 526, \"endX\": 4014, \"endY\": 531 } ]    ,[ { \"startX\": 8019, \"startY\": 1053, \"endX\": 8029, \"endY\": 1062 } ]    ,[ { \"startX\": 16039, \"startY\": 2106, \"endX\": 16059, \"endY\": 2125 } ]    ,[ { \"startX\": 32078, \"startY\": 4212, \"endX\": 32119, \"endY\": 4251 } ]    ,[ { \"startX\": 64156, \"startY\": 8425, \"endX\": 64238, \"endY\": 8503 } ]   ] } ";

    
    /// <summary>
    /// Genric function to return a static layer file. Will change in Part Two to be dynamic
    /// </summary>
    /// <returns></returns>
    private string GetGenericLayerFile()
    {
      // Todo. Part two this could be improved to a more taylored layer file based on projectid
      var dstr = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
      const string fname = @"TestData\layer.json";
      var fileInfo = new FileInfo(Path.Combine(dstr, fname));
      if (!fileInfo.Exists)
        throw new FileNotFoundException("[layer.json not in folder}]");
      try
      {
        return System.IO.File.ReadAllText(fileInfo.FullName);
      }
      catch (Exception e)
      {
        Console.WriteLine("[Data File Missing] {0}", e);
        throw new FileNotFoundException($"[layer.json not in {fileInfo.FullName}] ", e);
      }
    }
    

    /// <summary>
    /// Async call to make quantized mesh tile
    /// </summary>
    /// <param name="projectUId"> project id</param>
    /// <param name="filterUId">filter id</param>
    /// <param name="x">tile x coordinate</param>
    /// <param name="y">tile y coordinate</param>
    /// <param name="z">tile z coordinate</param>
    /// <returns></returns>
    private async Task<byte[]> FetchTile(Guid projectUId, Guid filterUId, int x, int y, int z)
    {
      var reqFilter = await GetCompactionFilter(projectUId, filterUId);
      var request = new QMTileRequest()
      {
        X = x,
        Y = y,
        Z = z,
        Filter = reqFilter,
        ProjectUid = projectUId
      };

      request.Validate();
      // Execute tile request
      var qmTileResult = await RequestExecutorContainerFactory.Build<QMTilesExecutor>(logger,
        configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders).ProcessAsync(request) as QMTileResult;

      return (qmTileResult == null) ? null : qmTileResult.TileData;

    }

    /*
    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="logger">LoggerFactory</param>
    /// <param name="configStore">Configuration Store</param>
    /// <param name="trexCompactionDataProxy">Trex Gateway production data proxy</param>
    public TerrainController(ILoggerFactory logger, IConfigurationStore configStore, ITRexCompactionDataProxy trexCompactionDataProxy)
    {
      this.logger = logger;
      log = this.logger.CreateLogger<TerrainController>();
      ConfigStore = configStore;
      TRexCompactionDataProxy = trexCompactionDataProxy;
    }
    */

    /// Default constructor.
    /// </summary>
    public TerrainController(
      IConfigurationStore configStore,
      IFileRepository fileRepo, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager, IProductionDataTileService tileService, IBoundingBoxHelper boundingBoxHelper) :
      base(configStore, fileImportProxy, settingsManager)
    {
     // this.fileRepo = fileRepo;
     // this.tileService = tileService;
     // this.boundingBoxHelper = boundingBoxHelper;
    }


    /// <summary>
    /// Request for a quantized mesh tile
    /// </summary>
    /// <param name="x">x tile coordinate</param>
    /// <param name="y">y tile coordinate</param>
    /// <param name="z">z tile coordinate</param>
    /// <param name="formatExtension">terrain ext</param>
    /// <param name="projectUid">Project UId</param>
    /// <param name="filterUId">Filter Id</param>
    /// <returns></returns>
    [HttpGet("v1/qmesh/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> Get(int x, int y, int z, string formatExtension, [FromQuery] Guid projectUid, [FromQuery] Guid filterUId)
    {

      Log.LogInformation("Get: " + Request.QueryString);
      //Log.LogDebug($"QMesh tile request params. x:{x},y:{y},z:{z}, ProjectId:{projectUid}, FilterUId:{filterUId} *");

      // var basicTile = await FetchTile(testDataPath, x, y, z);
      var qmTile = await FetchTile(projectUid, filterUId, x, y, z);
      if (qmTile != null)
      {
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentEncoding, ContentTypeConstants.ContentEncodingGzip); 
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentLength, qmTile.Length.ToString());
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentType, ContentTypeConstants.ApplicationOctetStream);
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentDisposition, $"attachment;filename={y}.terrain");
        return File(qmTile, ContentTypeConstants.ApplicationOctetStream);
      }

      Log.LogDebug($"Requested tile x:{x},y: {y},z:{z} for Project:{projectUid} was not found");
      return NotFound();
    }

    /// <summary>
    /// Returns layer.json that controls the layout of all future tile requets
    /// </summary>
    /// <returns></returns>
    [HttpGet("v1/qmesh/layer.json")]
    public string GetTRexLayerFile()
    {
      Log.LogInformation("GetTRexLayerFile: " + Request.QueryString);
      // Its possible this layer file could be custom made to control tile requests to a certain area around 
      // a projects boundary only. Hence reducing overall tile requets
         return GetGenericLayerFile();
      //return layer;
    }
  }
}

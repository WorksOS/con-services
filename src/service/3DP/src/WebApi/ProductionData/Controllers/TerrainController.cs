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
    private readonly ITRexCompactionDataProxy TRexCompactionDataProxy;


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
      var request = new QMTileRequest()
      {
        X = x,
        Y = y,
        Z = z,
        CallId = new Guid(),

        // Todo setup correct filterUId filter in part two
        Filter = new FilterResult(),
        ProjectUid = projectUId
      };

      request.Validate();
      // Execute tile reuest
      var qmTileResult = await RequestExecutorContainerFactory.Build<QMTilesExecutor>(logger,
        configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders).ProcessAsync(request) as QMTileResult;

      return (qmTileResult == null) ? null : qmTileResult.TileData;

    }

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

      log.LogInformation("Get: " + Request.QueryString);
      log.LogDebug($"QMesh tile request params. x:{x},y:{y},z:{z}, ProjectId:{projectUid}, FilterUId:{filterUId} *");

      // var basicTile = await FetchTile(testDataPath, x, y, z);
      var basicTile = await FetchTile(projectUid, filterUId, x, y, z);
      if (basicTile != null)
      {
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentEncoding, ContentTypeConstants.ContentEncodingGzip); // already compressed on disk
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentLength, basicTile.Length.ToString());
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentType, ContentTypeConstants.ApplicationOctetStream);
        HttpContext.Response.Headers.Add(ContentTypeConstants.ContentDisposition, $"attachment;filename={y}.terrain");
        return File(basicTile, ContentTypeConstants.ApplicationOctetStream);
      }

      log.LogDebug($"Requested tile x:{x},y: {y},z:{z} for Project:{projectUid} was not found");
      return NotFound();
    }

    /// <summary>
    /// Returns layer.json that controls the layout of all future tile requets
    /// </summary>
    /// <returns></returns>
    [HttpGet("v1/qmesh/layer.json")]
    public string GetTRexLayerFile()
    {

      // Its possible this layer file could be custom made to control tile requests to a certain area around 
      // a projects boundary only. Hence reducing overall tile requets
      return GetGenericLayerFile();
    }
  }
}

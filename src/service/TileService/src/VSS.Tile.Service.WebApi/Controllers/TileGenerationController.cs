using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Pegasus.Client;
using VSS.Pegasus.Client.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.WebApi.Controllers
{
  /// <summary>
  /// Web API endpoints for generating tiles.
  /// </summary>
  public class TileGenerationController : Controller
  {
    /// <summary>
    /// Client for Pegasus API used to generate tiles.
    /// </summary>
    private readonly IPegasusClient pegasusClient;
    /// <summary>
    /// For getting application bearer token
    /// </summary>
    private readonly ITPaaSApplicationAuthentication authn;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TileGenerationController(IPegasusClient pegasusClient)
    {
      this.pegasusClient = pegasusClient;
    }

    /// <summary>
    /// Generates tiles for the DXF file.
    /// </summary>
    /// <param name="dcFileName">The path and name of coordinate system file used to locate the DXF linework on the map</param>
    /// <param name="dxfFileName">The path and name of the DXF file</param>
    /// <param name="dxfUnitsType">The units for the DXF file</param>
    /// <returns></returns>
    [Route("/internal/v1/generatedxftiles")]
    [HttpGet]
    public Task<TileMetadata> GenerateDxfTiles([FromQuery] string dcFileName, [FromQuery] string dxfFileName, [FromQuery] DxfUnitsType dxfUnitsType)
    {
      if (string.IsNullOrEmpty(dcFileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing coordinate system file name"));
      }
      if (string.IsNullOrEmpty(dxfFileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing DXF file name"));
      }
      var customHeaders = new Dictionary<string, string>
      {
        {"Content-Type", "application/json"},
        {"Authorization", $"Bearer {authn.GetApplicationBearerToken()}"}
      };
      return pegasusClient.GenerateDxfTiles(dcFileName, dxfFileName, dxfUnitsType, customHeaders);
    }
  }
}

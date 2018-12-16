using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Pegasus.Client.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Proxies
{
  public class TileServiceProxy : BaseProxy, ITileServiceProxy
  {
    public TileServiceProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(
      configurationStore, logger, cache)
    {
    }

    public async Task<TileMetadata> GenerateDxfTiles(string dcFileName, string dxfFileName, DxfUnitsType dxfUnitsType,
      IDictionary<string, string> customHeaders)
    {
      log.LogDebug($"{nameof(GenerateDxfTiles)}: dcFileName={dcFileName}, dxfFileName={dxfFileName}, dxfUnitsType={dxfUnitsType}");

      Dictionary<string, string> parameters = new Dictionary<string, string>
      {
        { "dcFileName", dcFileName }, {"dxfFileName" , dxfFileName }, { "dxfUnitsType", dxfUnitsType.ToString() }
      };

      var queryParams = $"?{new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result}";

      TileMetadata response = await SendRequest<TileMetadata>("TILE_API_URL",
        string.Empty, customHeaders, "/generatedxftiles", HttpMethod.Get, queryParams);

      return response;
    }
  }
}

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;

namespace VSS.Productivity3D.Productivity3D.Proxy
{  
  /// <summary>
  /// Proxy for Productivity services.
  /// ServiceDiscovery covers 2 Productivity3D Coord endpoints:
  /// 1) COORDSYSPOST_API_URL HttpPost "api/v1/coordsystem"
  ///      https://api.trimble.com/t/trimble.com/vss-3dproductivity/1.0/coordsystem
  ///      http://localhost:5001/api/v1/coordsystem
  ///
  /// 2) COORDSYSVALIDATE_API_URL HttpPost "api/v1/coordsystem/validation"
  ///      https://api-stg.trimble.com/t/trimble.com/vss-dev-3dproductivity/1.0/coordsystem/validation
  ///      http://localhost:5001/api/v1/coordsystem/validation
  /// 
  /// Called from UI only, i.e. no ServiceDiscovery required
  /// 1) HttpGet "api/v1/projects/{projectId}/coordsystem"
  /// 2) HttpGet "api/v2/projects/{projectUid}/coordsystem"
  /// </summary>
  public class Productivity3dV1ProxyCoord : BaseServiceDiscoveryProxy, IProductivity3dV1ProxyCoord
  {
    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Productivity3D;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "PRODUCTIVITY3D_COORD_CACHE_LIFE"; // not used

    public Productivity3dV1ProxyCoord(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// Validates the CoordinateSystem for the project.
    ///  COORDSYSVALIDATE_API_URL HttpPost "api/v1/coordsystem/validation"
    /// </summary>
    public async Task<CoordinateSystemSettingsResult> CoordinateSystemValidate(byte[] coordinateSystemFileContent, string coordinateSystemFileName, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(CoordinateSystemValidate)} coordinateSystemFileName: {coordinateSystemFileName}");
      var payLoadToSend = new CoordinateSystemFileValidationRequest(coordinateSystemFileContent, coordinateSystemFileName);

      return await CoordSystemPost(JsonConvert.SerializeObject(payLoadToSend), customHeaders, "/coordsystem/validation");
    }

    /// <summary>
    /// Validates and posts to TRex/Raptor, the CoordinateSystem for the project.
    ///  COORDSYSPOST_API_URL HttpPost "api/v1/coordsystem"
    /// </summary>
    public async Task<CoordinateSystemSettingsResult> CoordinateSystemPost(long legacyProjectId, byte[] coordinateSystemFileContent, string coordinateSystemFileName, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(CoordinateSystemPost)} coordinateSystemFileName: {coordinateSystemFileName}");
      var payLoadToSend = CoordinateSystemFile.CreateCoordinateSystemFile(legacyProjectId, coordinateSystemFileContent, coordinateSystemFileName);

      return await CoordSystemPost(JsonConvert.SerializeObject(payLoadToSend), customHeaders, "/coordsystem");
    }

    /// <summary>
    /// Posts the coordinate system to Raptor
    /// </summary>
    private async Task<CoordinateSystemSettingsResult> CoordSystemPost(string payload, IDictionary<string, string> customHeaders, string route)
    {
      using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        var result = await SendMasterDataItemServiceDiscoveryNoCache<CoordinateSystemSettingsResult>(route, customHeaders, HttpMethod.Post, payload: stream);
        if (result != null)
          return result;
      }

      log.LogDebug($"{nameof(CoordSystemPost)} Failed to post request");
      return null;
    }


    /// <summary>
    /// Execute a generic request against v1 raptor endpoint
    /// </summary>
    public async Task<T> ExecuteGenericV1Request<T>(string route, object payload, IDictionary<string, string> customHeaders = null)
      where T : class, IMasterDataModel 
    {
      log.LogDebug($"{nameof(ExecuteGenericV1Request)} route: {route}");
      using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))))
      {
        var result = await SendMasterDataItemServiceDiscoveryNoCache<T>(route, customHeaders, HttpMethod.Post, payload: stream);
        if (result != null)
          return result;
      }

      log.LogDebug($"{nameof(ExecuteGenericV1Request)} Failed to post request");
      return null;
    }
  }
}

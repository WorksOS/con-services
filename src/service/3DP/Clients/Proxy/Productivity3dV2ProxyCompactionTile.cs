using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.Productivity3D.Proxy
{
  public class Productivity3dV2ProxyCompactionTile : Productivity3dV2Proxy, IProductivity3dV2ProxyCompactionTile
  {
    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Productivity3D;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V2;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "PRODUCTIVITY3D_COMPACTION_CACHE_LIFE"; // not used

    public Productivity3dV2ProxyCompactionTile(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// Gets a production data tile from the 3dpm WMS service.
    /// </summary>
    public async Task<byte[]> GetProductionDataTile(Guid projectUid, Guid? filterUid, Guid? cutFillDesignUid, ushort width, ushort height,
      string bbox, DisplayMode mode, Guid? baseUid, Guid? topUid, VolumeCalcType? volCalcType, IDictionary<string, string> customHeaders = null, bool explicitFilters = false)
    {
      log.LogDebug($"{nameof(GetProductionDataTile)}: projectUid={projectUid}, filterUid={filterUid}, width={width}, height={height}, mode={mode}, bbox={bbox}, baseUid={baseUid}, topUid={topUid}, volCalcType={volCalcType}, cutFillDesignUid={cutFillDesignUid}");

      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("SERVICE", "WMS"), new KeyValuePair<string, string>("VERSION", "1.3.0"),
        new KeyValuePair<string, string>("REQUEST", "GetMap"), new KeyValuePair<string, string>("FORMAT", ContentTypeConstants.ImagePng),
        new KeyValuePair<string, string>("TRANSPARENT", "true"), new KeyValuePair<string, string>("LAYERS", "Layers"),
        new KeyValuePair<string, string>("CRS", "EPSG:4326"), new KeyValuePair<string, string>("STYLES", string.Empty),
        new KeyValuePair<string, string>( "projectUid", projectUid.ToString()), new KeyValuePair<string, string>("mode", mode.ToString()),
        new KeyValuePair<string, string>("width", width.ToString()), new KeyValuePair<string, string>("height", height.ToString()),
        new KeyValuePair<string, string>("bbox", bbox)
      };
      if (filterUid.HasValue)
        queryParams.Add(new KeyValuePair<string, string>("filterUid", filterUid.ToString()));

      if (cutFillDesignUid.HasValue)
        queryParams.Add(new KeyValuePair<string, string>("cutFillDesignUid", cutFillDesignUid.ToString()));

      if (baseUid.HasValue)
        queryParams.Add(new KeyValuePair<string, string>("volumeBaseUid", baseUid.ToString()));

      if (topUid.HasValue)
        queryParams.Add(new KeyValuePair<string, string>("volumeTopUid", topUid.ToString()));

      if (volCalcType.HasValue)
        queryParams.Add(new KeyValuePair<string, string>("volumeCalcType", volCalcType.ToString()));

      queryParams.Add(new KeyValuePair<string, string>("explicitFilters", explicitFilters.ToString()));
      var stream = await GetMasterDataStreamItemServiceDiscoveryNoCache
        ("/productiondatatiles/png", customHeaders, HttpMethod.Get, queryParams);

      byte[] byteArray = new byte[stream.Length];
      stream.Read(byteArray, 0, (int)stream.Length);
      return byteArray;
    }

    /// <summary>
    /// Get the points for all active alignment files for a project.
    /// </summary>
    public async Task<PointsListResult> GetAlignmentPointsList(Guid projectUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetAlignmentPointsList)} projectUid: {projectUid}");
      var queryParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("projectUid", projectUid.ToString()) };
      var result = await GetMasterDataItemServiceDiscoveryNoCache<PointsListResult>
        ("/raptor/alignmentpointslist", customHeaders, queryParams);
      if (result != null)
        return result;

      log.LogDebug($"{nameof(GetAlignmentPointsList)} Failed to get alignment BoundaryPoints");
      return null;
    }

    /// <summary>
    /// Get the boundary points for a design for a project.
    /// </summary>
    public async Task<PointsListResult> GetDesignBoundaryPoints(Guid projectUid, Guid designUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDesignBoundaryPoints)} projectUid: {projectUid}, designUid: {designUid}");

      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("projectUid", projectUid.ToString()),
        new KeyValuePair<string, string>("designUid", designUid.ToString())
      };
      var result = await GetMasterDataItemServiceDiscoveryNoCache<PointsListResult>
        ("/raptor/designboundarypoints", customHeaders, queryParameters: queryParams);
      if (result != null)
        return result;

      log.LogDebug($"{nameof(GetDesignBoundaryPoints)} Failed to get designBoundaryPoints");
      return null;
    }

    /// <summary>
    /// Get the boundary points for the requested filters for a project if they have a spatial filter
    /// </summary>
    public async Task<PointsListResult> GetFilterPointsList(Guid projectUid, Guid? filterUid, Guid? baseUid, Guid? topUid, FilterBoundaryType boundaryType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetFilterPointsList)} projectUid={projectUid}, filterUid={filterUid}, baseUid={baseUid}, topUid={topUid}, boundaryType={boundaryType}");

      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("projectUid", projectUid.ToString()),
        new KeyValuePair<string, string>("filterUid", filterUid.ToString()),
        new KeyValuePair<string, string>("baseUid", baseUid.ToString()),
        new KeyValuePair<string, string>("topUid", topUid.ToString()),
        new KeyValuePair<string, string>("boundaryType", boundaryType.ToString())
      };
      var result = await GetMasterDataItemServiceDiscoveryNoCache<PointsListResult>
        ("/raptor/filterpointslist", customHeaders, queryParameters: queryParams);
      if (result != null)
        return result;

      log.LogDebug($"{nameof(GetFilterPointsList)} Failed to get streamed results");
      return null;
    }


    /// <summary>
    /// Gets a "best fit" bounding box for the requested parameters.
    /// </summary>
    /// <returns>The bounding box of the tile in decimal degrees: bottom left corner lat/lng and top right corner lat/lng</returns>
    public async Task<string> GetBoundingBox(Guid projectUid, TileOverlayType[] overlays, Guid? filterUid, Guid? cutFillDesignUid, Guid? baseUid,
      Guid? topUid, VolumeCalcType? volCalcType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetBoundingBox)} projectUid={projectUid}, overlays={JsonConvert.SerializeObject(overlays)}, filterUid={filterUid}, baseUid={baseUid}, topUid={topUid}, volCalcType={volCalcType}, cutFillDesignUid={cutFillDesignUid}");

      var queryParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("projectUid", projectUid.ToString()) };
      if (filterUid.HasValue)
        queryParams.Add(new KeyValuePair<string, string>("filterUid", filterUid.ToString()));
      if (cutFillDesignUid.HasValue)
        queryParams.Add(new KeyValuePair<string, string>("cutFillDesignUid", cutFillDesignUid.ToString()));
      if (baseUid.HasValue)
        queryParams.Add(new KeyValuePair<string, string>("baseUid", baseUid.ToString()));
      if (topUid.HasValue)
        queryParams.Add(new KeyValuePair<string, string>("topUid", topUid.ToString()));
      if (volCalcType.HasValue)
        queryParams.Add(new KeyValuePair<string, string>("volumeCalcType", volCalcType.ToString()));
      queryParams.Add(new KeyValuePair<string, string>("overlays", string.Join("&overlays=", overlays)));

      var stream = await GetMasterDataStreamItemServiceDiscoveryNoCache
        ("/raptor/boundingbox", customHeaders, HttpMethod.Get, queryParameters: queryParams);
      if (stream != null)
      {
        var reader = new StreamReader(stream);
        return reader.ReadToEnd();
      }

      log.LogDebug($"{nameof(GetBoundingBox)} Failed to get bounding box");
      return null;
    }
  }
}

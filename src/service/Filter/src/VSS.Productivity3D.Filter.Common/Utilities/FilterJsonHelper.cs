using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.Filter.Abstractions.Models;
using DbFilter = VSS.MasterData.Repositories.DBModels.Filter;

namespace VSS.Productivity3D.Filter.Common.Utilities
{
  public class FilterJsonHelper
  {
    public static async Task ParseFilterJson(ProjectData project, IEnumerable<DbFilter> filters, IRaptorProxy raptorProxy,
      IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      if (filters == null) return;

      foreach (var filter in filters)
        await FixupFilterValues(project, filter, raptorProxy, assetResolverProxy, customHeaders);
    }

    public static async Task ParseFilterJson(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy,
      IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      if (filter == null) return;

      await FixupFilterValues(project, filter, raptorProxy, assetResolverProxy, customHeaders);
    }

    public static async Task ParseFilterJson(ProjectData project, FilterDescriptor filter, IRaptorProxy raptorProxy,
      IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      if (filter == null) return;

      var processFilterJson = await ProcessFilterJson(project, filter.FilterJson, raptorProxy, assetResolverProxy, customHeaders);

      filter.FilterJson = processFilterJson.filterJson;
      filter.ContainsBoundary = processFilterJson.containsBoundary;
    }

    private static async Task<(string filterJson, bool containsBoundary)> ProcessFilterJson(ProjectData project, string filterJson,
      IRaptorProxy raptorProxy, IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      var filterObj = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterJson);

      // date timezone changes
      filterObj.ApplyDateRange(project?.IanaTimeZone);

      if (filterObj.DateRangeType == DateRangeType.ProjectExtents)
      {
        // get project productionData data extents from 3dpm
        var statistics = raptorProxy?.GetProjectStatistics(Guid.Parse(project?.ProjectUid), customHeaders).Result;
        filterObj.StartUtc = statistics?.startTime;
        filterObj.EndUtc = statistics?.endTime;
      }

      // The UI needs to know the start date for specified ranges, this is actually the range data will be returned for
      if (filterObj.AsAtDate == true)
      {
        var statistics = raptorProxy?.GetProjectStatistics(Guid.Parse(project?.ProjectUid), customHeaders).Result;
        filterObj.StartUtc = statistics?.startTime;
        filterObj.DateRangeType = DateRangeType.Custom;
      }

      // pair up AssetUids and legacyAssetIds in contributingMachines
      await PairUpAssetIdentifiersAsync(filterObj.ContributingMachines, assetResolverProxy, customHeaders);

      return (JsonConvert.SerializeObject(filterObj), filterObj.ContainsBoundary);
    }

    private static async Task FixupFilterValues(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy,
      IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      var processFilterJson = await ProcessFilterJson(project, filter.FilterJson, raptorProxy, assetResolverProxy, customHeaders);

      filter.FilterJson = processFilterJson.filterJson;
    }

    // It is likely we have a combination of filters stored for a project.
    // Older ones will have legacyAssetId (assetUid = null) and more recent ones will have AssetUid (legacyAssetID == -1)
    private static async Task PairUpAssetIdentifiersAsync(List<MachineDetails> machines,
      IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      if (machines == null || !machines.Any())
        return;

      var assetUids = new List<Guid>(machines.Where(a => a.AssetUid.HasValue && a.AssetUid.Value != Guid.Empty && !a.IsJohnDoe
                                                                             && a.AssetId <= 0
                                                   ).Select(a => a.AssetUid.Value).Distinct());
      if (assetUids.Count > 0)
      {
        // assetMatch will return rows of found Uids.
        var assetMatchingResult = await assetResolverProxy.GetMatchingAssets(assetUids, customHeaders);
        foreach (var assetMatch in assetMatchingResult)
        {
          if (assetMatch.Value > 0)
            foreach (var assetOnDesignPeriod in machines.FindAll(x => x.AssetUid == assetMatch.Key))
              assetOnDesignPeriod.AssetId = assetMatch.Value;
        }
      }

      var assetIds = new List<long>(machines.Where(a => a.AssetId > 0 && !a.IsJohnDoe
                                                                      && (!a.AssetUid.HasValue ||
                                                                          a.AssetUid.Value == Guid.Empty)
                                                   ).Select(a => a.AssetId).Distinct());
      if (assetIds.Count > 0)
      {
        // assetMatch will only return rows of found legacyAssetIds.
        var assetMatchingResult = await assetResolverProxy.GetMatchingAssets(assetIds, customHeaders);
        foreach (var assetMatch in assetMatchingResult)
          {
            if (assetMatch.Value > 0) // machineId of 0/-1 may occur for >1 AssetUid
              foreach (var assetOnDesignPeriod in machines.FindAll(x => x.AssetId == assetMatch.Value))
                assetOnDesignPeriod.AssetUid = assetMatch.Key;
        }
      }
    }
  }
}

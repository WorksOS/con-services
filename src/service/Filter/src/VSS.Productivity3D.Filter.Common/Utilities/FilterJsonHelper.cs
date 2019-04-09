using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
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
    public static void ParseFilterJson(ProjectData project, IEnumerable<DbFilter> filters, IRaptorProxy raptorProxy,
      IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      if (filters == null)
      {
        return;
      }

      foreach (var filter in filters)
      {
        FixupFilterValues(project, filter, raptorProxy, assetResolverProxy, customHeaders);
      }
    }

    public static void ParseFilterJson(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy,
      IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      if (filter == null)
      {
        return;
      }

      FixupFilterValues(project, filter, raptorProxy, assetResolverProxy, customHeaders);
    }

    public static void ParseFilterJson(ProjectData project, FilterDescriptor filter, IRaptorProxy raptorProxy,
      IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      if (filter == null)
      {
        return;
      }

      var processFilterJson =
        ProcessFilterJson(project, filter.FilterJson, raptorProxy, assetResolverProxy, customHeaders);

      filter.FilterJson = processFilterJson.filterJson;
      filter.ContainsBoundary = processFilterJson.containsBoundary;
    }

    private static (string filterJson, bool containsBoundary) ProcessFilterJson(ProjectData project, string filterJson,
      IRaptorProxy raptorProxy, IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      var filterObj = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterJson);

      // FixupFilterValues
      filterObj.ApplyDateRange(project?.IanaTimeZone);

      if (filterObj.DateRangeType == DateRangeType.ProjectExtents)
      {
        //get extents from 3d pm
        var statistics = raptorProxy?.GetProjectStatistics(Guid.Parse(project?.ProjectUid), customHeaders).Result;
        filterObj.StartUtc = statistics?.startTime;
        filterObj.EndUtc = statistics?.endTime;
      }

      //The UI needs to know the start date for specified ranges, this is actually the range data will be returned for
      if (filterObj.AsAtDate == true)
      {
        var statistics = raptorProxy?.GetProjectStatistics(Guid.Parse(project?.ProjectUid), customHeaders).Result;
        filterObj.StartUtc = statistics?.startTime;
        filterObj.DateRangeType = DateRangeType.Custom;
      }

      // pair up AssetUids and legacyAssetIds
      PairUpAssetIdentifiersAsync(filterObj.ContributingMachines, assetResolverProxy, customHeaders);

      return (JsonConvert.SerializeObject(filterObj), filterObj.ContainsBoundary);
    }

    private static void FixupFilterValues(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy,
      IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      var processFilterJson = ProcessFilterJson(project, filter.FilterJson, raptorProxy, assetResolverProxy, customHeaders);

      filter.FilterJson = processFilterJson.filterJson;
    }

    // It is likely we have a combination of filters stored for a project.
    // Older ones will have legacyAssetId (assetUid = null) and more recent ones will have AssetUid (legacyAssetID == -1)
    private static void PairUpAssetIdentifiersAsync(List<MachineDetails> machines,
      IAssetResolverProxy assetResolverProxy, IDictionary<string, string> customHeaders)
    {
      if (machines == null || !machines.Any())
        return;


      // assetMatch will return rows only if Uids found.
      var assetUids = new List<Guid>(machines.Where(a => a.AssetUid.HasValue && a.AssetUid.Value != Guid.Empty && !a.IsJohnDoe
                                                                             && a.AssetId <= 0
                                                   ).Select(a => a.AssetUid.Value).Distinct());
      if (assetUids.Count > 0)
      {
        var assetMatchingResult = Task.Run(() => assetResolverProxy.GetMatchingAssets(assetUids, customHeaders)).Result.ToList();
        foreach (var assetMatch in assetMatchingResult)
        {
          if (assetMatch.Value > 0)
            foreach (var assetOnDesignPeriod in machines.FindAll(x => x.AssetUid == assetMatch.Key))
              assetOnDesignPeriod.AssetId = assetMatch.Value;
        }
      }

      // assetMatch will only return rows if Uids found for the legacyAssetIds
      var assetIds = new List<long>(machines.Where(a => a.AssetId > 0 && !a.IsJohnDoe
                                                                      && (!a.AssetUid.HasValue ||
                                                                          a.AssetUid.Value == Guid.Empty)
                                                   ).Select(a => a.AssetId).Distinct());
      if (assetIds.Count > 0)
      {
        var assetMatchingResult = Task.Run(() => assetResolverProxy.GetMatchingAssets(assetIds, customHeaders)).Result.ToList();
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

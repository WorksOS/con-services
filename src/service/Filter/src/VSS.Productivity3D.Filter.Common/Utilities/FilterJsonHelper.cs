using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using DbFilter = VSS.MasterData.Repositories.DBModels.Filter;

namespace VSS.Productivity3D.Filter.Common.Utilities
{
  public class FilterJsonHelper
  {
    public static async Task ParseFilterJson(ProjectData project, IEnumerable<DbFilter> filters, IProductivity3dProxy productivity3DProxy, IDictionary<string, string> customHeaders)
    {
      if (filters == null) return;

      foreach (var filter in filters)
        await FixupFilterValues(project, filter, productivity3DProxy, customHeaders);
    }

    public static async Task ParseFilterJson(ProjectData project, DbFilter filter, IProductivity3dProxy productivity3DProxy, IDictionary<string, string> customHeaders)
    {
      if (filter == null) return;

      await FixupFilterValues(project, filter, productivity3DProxy, customHeaders);
    }

    public static async Task ParseFilterJson(ProjectData project, FilterDescriptor filter, IProductivity3dProxy productivity3DProxy, IDictionary<string, string> customHeaders)
    {
      if (filter == null) return;

      var processFilterJson = await ProcessFilterJson(project, filter.FilterJson, productivity3DProxy, customHeaders);

      filter.FilterJson = processFilterJson.filterJson;
      filter.ContainsBoundary = processFilterJson.containsBoundary;
    }

    private static async Task<(string filterJson, bool containsBoundary)> ProcessFilterJson(ProjectData project, string filterJson,
      IProductivity3dProxy productivity3DProxy, IDictionary<string, string> customHeaders)
    {
      var filterObj = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterJson);

      // date timezone changes
      filterObj.ApplyDateRange(project?.IanaTimeZone);

      if (filterObj.DateRangeType == DateRangeType.ProjectExtents)
      {
        // get project productionData data extents from 3dpm
        var statistics = productivity3DProxy?.GetProjectStatistics(Guid.Parse(project?.ProjectUid), customHeaders).Result;
        filterObj.StartUtc = statistics?.startTime;
        filterObj.EndUtc = statistics?.endTime;
      }

      // The UI needs to know the start date for specified ranges, this is actually the range data will be returned for
      if (filterObj.AsAtDate == true)
      {
        var statistics = productivity3DProxy?.GetProjectStatistics(Guid.Parse(project?.ProjectUid), customHeaders).Result;
        filterObj.StartUtc = statistics?.startTime;
        filterObj.DateRangeType = DateRangeType.Custom;
      }

      // pair up AssetUids and legacyAssetIds in contributingMachines
      await PairUpAssetIdentifiers(project?.ProjectUid, filterObj.ContributingMachines, productivity3DProxy, customHeaders);

      return (JsonConvert.SerializeObject(filterObj), filterObj.ContainsBoundary);
    }

    private static async Task FixupFilterValues(ProjectData project, DbFilter filter, IProductivity3dProxy productivity3DProxy, IDictionary<string, string> customHeaders)
    {
      var processFilterJson = await ProcessFilterJson(project, filter.FilterJson, productivity3DProxy, customHeaders);

      filter.FilterJson = processFilterJson.filterJson;
    }

    // It is likely we have a combination of filters stored for a project.
    // Older ones will have legacyAssetId (assetUid = null) and more recent ones will have AssetUid (legacyAssetID == -1)
    private static async Task PairUpAssetIdentifiers(string projectUid, List<MachineDetails> machines,
      IProductivity3dProxy productivity3DProxy, IDictionary<string, string> customHeaders)
    {
      if (machines == null || !machines.Any())
        return;

      var route = $"/projects/{projectUid}/machines";
      var assetList = await productivity3DProxy.ExecuteGenericV2Request<MachineExecutionResult>(route, HttpMethod.Get, null, customHeaders);

      foreach (var assetMatch in assetList.MachineStatuses.Where(a => a.AssetUid.HasValue && a.AssetUid.Value != Guid.Empty && a.AssetId > 0))
      {
        foreach (var assetOnDesignPeriod in machines.FindAll(a => a.AssetUid == assetMatch.AssetUid && a.AssetId < 1))
          assetOnDesignPeriod.AssetId = assetMatch.AssetId;

        foreach (var assetOnDesignPeriod in machines.FindAll(a => a.AssetId == assetMatch.AssetId && (!a.AssetUid.HasValue || a.AssetUid.Value == Guid.Empty)))
          assetOnDesignPeriod.AssetUid = assetMatch.AssetUid;
      }
    }
  }
}

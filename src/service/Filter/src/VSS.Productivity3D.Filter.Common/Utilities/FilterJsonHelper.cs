using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using DbFilter = VSS.MasterData.Repositories.DBModels.Filter;

namespace VSS.Productivity3D.Filter.Common.Utilities
{
  public class FilterJsonHelper
  {
    public static void ParseFilterJson(ProjectData project, IEnumerable<DbFilter> filters, IRaptorProxy raptorProxy, IDictionary<string, string> customHeaders)
    {
      if (filters == null) { return; }

      foreach (var filter in filters)
      {
        GenerateIanaBasedDateTime(project, filter, raptorProxy, customHeaders);
      }
    }

    public static void ParseFilterJson(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy, IDictionary<string, string> customHeaders)
    {
      if (filter == null) { return; }

      GenerateIanaBasedDateTime(project, filter, raptorProxy, customHeaders);
    }

    public static void ParseFilterJson(ProjectData project, FilterDescriptor filter, IRaptorProxy raptorProxy, IDictionary<string, string> customHeaders)
    {
      if (filter == null) { return; }

      var processFilterJson = ProcessFilterJson(project, filter.FilterJson, raptorProxy, customHeaders);

      filter.FilterJson = processFilterJson.filterJson;
      filter.ContainsBoundary = processFilterJson.containsBoundary;
    }

    private static void GenerateIanaBasedDateTime(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy, IDictionary<string, string> customHeaders)
    {
      var processFilterJson = ProcessFilterJson(project, filter.FilterJson, raptorProxy, customHeaders);

      filter.FilterJson = processFilterJson.filterJson;
    }

    private static (string filterJson, bool containsBoundary) ProcessFilterJson(ProjectData project, string filterJson, IRaptorProxy raptorProxy, IDictionary<string, string> customHeaders)
    {
      var filterObj = JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterJson);

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

      return (JsonConvert.SerializeObject(filterObj), filterObj.ContainsBoundary);
    }
  }
}

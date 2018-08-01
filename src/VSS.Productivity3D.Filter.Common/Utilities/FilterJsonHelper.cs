using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using DbFilter = VSS.MasterData.Repositories.DBModels.Filter;

namespace VSS.Productivity3D.Filter.Common.Utilities
{
  public class FilterJsonHelper
  {
    public static void ParseFilterJson(ProjectData project, IEnumerable<DbFilter> filters, IRaptorProxy raptorProxy, IDictionary<string, string> customHeaders)
    {
      if (filters == null)
      {
        return;
      }

      foreach (var filter in filters)
      {
        GenerateIanaBasedDateTime(project, filter, raptorProxy, customHeaders);
      }
    }

    public static void ParseFilterJson(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy, IDictionary<string, string> customHeaders)
    {
      if (filter == null)
      {
        return;
      }

      GenerateIanaBasedDateTime(project, filter, raptorProxy, customHeaders);
    }

    public static void ParseFilterJson(ProjectData project, FilterDescriptor filter, IRaptorProxy raptorProxy, IDictionary<string, string> customHeaders)
    {
      if (filter == null)
      {
        return;
      }

      filter.FilterJson = ProcessFilterJson(project, filter.FilterJson, raptorProxy, customHeaders);
    }

    private static void GenerateIanaBasedDateTime(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy, IDictionary<string, string> customHeaders)
    {
      filter.FilterJson = ProcessFilterJson(project, filter, raptorProxy, customHeaders);
    }

    private static string ProcessFilterJson(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy, IDictionary<string, string> customHeaders)
    {
      return ProcessFilterJson(project, filter.FilterJson, raptorProxy, customHeaders);
    }

    private static string ProcessFilterJson(ProjectData project, string filterJson, IRaptorProxy raptorProxy, IDictionary<string, string> customHeaders)
    {
      try
      {
        MasterData.Models.Models.Filter filterObj = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filterJson);

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

        return JsonConvert.SerializeObject(filterObj);
      }
      catch (Exception exception)
      {
        return string.Empty;
      }
    }
  }
}

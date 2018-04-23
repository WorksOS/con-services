using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using DbFilter = VSS.MasterData.Repositories.DBModels.Filter;

namespace VSS.Productivity3D.Filter.Common.Utilities
{
  public class FilterJsonHelper
  {
    public static void ParseFilterJson(ProjectData project, IEnumerable<DbFilter> filters, IRaptorProxy raptorProxy)
    {
      if (filters == null)
      {
        return;
      }

      foreach (var filter in filters)
      {
        GenerateIanaBasedDateTime(project, filter, raptorProxy);
      }
    }

    public static void ParseFilterJson(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy)
    {
      if (filter == null)
      {
        return;
      }

      GenerateIanaBasedDateTime(project, filter, raptorProxy);
    }

    public static void ParseFilterJson(ProjectData project, FilterDescriptor filter, IRaptorProxy raptorProxy)
    {
      if (filter == null)
      {
        return;
      }

      filter.FilterJson = ProcessFilterJson(project, filter.FilterJson, raptorProxy);
    }

    private static void GenerateIanaBasedDateTime(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy)
    {
      filter.FilterJson = ProcessFilterJson(project, filter, raptorProxy);
    }

    private static string ProcessFilterJson(ProjectData project, DbFilter filter, IRaptorProxy raptorProxy)
    {
      return ProcessFilterJson(project, filter.FilterJson, raptorProxy);
    }

    private static string ProcessFilterJson(ProjectData project, string filterJson, IRaptorProxy raptorProxy)
    {
      try
      {
        MasterData.Models.Models.Filter filterObj = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filterJson);
   
        filterObj.ApplyDateRange(project?.IanaTimeZone);

        if (filterObj.DateRangeType == DateRangeType.ProjectExtents)
        {
          //get extents from 3d pm
          ProjectStatisticsResult statistics = raptorProxy?.GetProjectStatistics(Guid.Parse(project?.ProjectUid)).Result;
          filterObj.StartUtc = statistics?.startTime;
          filterObj.EndUtc = statistics?.endTime;
        }

        return JsonConvert.SerializeObject(filterObj);
      }
      catch(Exception)
      {
        return string.Empty;
      }
    }
  }
}
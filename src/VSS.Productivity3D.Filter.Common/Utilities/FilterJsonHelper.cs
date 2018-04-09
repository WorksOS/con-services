using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Common.Extensions;
using DbFilter = VSS.MasterData.Repositories.DBModels.Filter;

namespace VSS.Productivity3D.Filter.Common.Utilities
{
  public class FilterJsonHelper
  {
    public static void ParseFilterJson(ProjectData project, IEnumerable<DbFilter> filters)
    {
      if (filters == null)
      {
        return;
      }

      foreach (var filter in filters)
      {
        GenerateIanaBasedDateTime(project, filter);
      }
    }

    public static void ParseFilterJson(ProjectData project, DbFilter filter)
    {
      if (filter == null)
      {
        return;
      }

      GenerateIanaBasedDateTime(project, filter);
    }

    public static void ParseFilterJson(ProjectData project, FilterDescriptor filter)
    {
      if (filter == null)
      {
        return;
      }

      filter.FilterJson = ProcessFilterJson(project, filter.FilterJson);
    }

    private static void GenerateIanaBasedDateTime(ProjectData project, DbFilter filter)
    {
      filter.FilterJson = ProcessFilterJson(project, filter);
    }

    private static string ProcessFilterJson(ProjectData project, DbFilter filter)
    {
      return ProcessFilterJson(project, filter.FilterJson);
    }

    private static string ProcessFilterJson(ProjectData project, string filterJson)
    {
      try
      {
        MasterData.Models.Models.Filter filterObj = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filterJson);
   
        filterObj.ApplyDateRange(project?.IanaTimeZone);
        
        return JsonConvert.SerializeObject(filterObj);
      }
      catch(Exception)
      {
        return string.Empty;
      }
    }
  }
}
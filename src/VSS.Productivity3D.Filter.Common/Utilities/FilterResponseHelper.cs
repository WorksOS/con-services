using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Common.Extensions;
using DbFilter = VSS.MasterData.Repositories.DBModels.Filter;

namespace VSS.Productivity3D.Filter.Common.Utilities
{
  public class FilterResponseHelper
  {
    public static void SetStartEndDates(ProjectData project, IEnumerable<DbFilter> filters)
    {
      if (project == null || filters == null)
      {
        return;
      }

      foreach (var filter in filters)
      {
        GenerateIanaBasedDateTime(project.IanaTimeZone, filter);
      }
    }

    public static void SetStartEndDates(ProjectData project, DbFilter filter)
    {
      if (project == null || filter == null)
      {
        return;
      }

      GenerateIanaBasedDateTime(project.IanaTimeZone, filter);
    }

    public static void SetStartEndDates(ProjectData project, FilterDescriptor filter)
    {
      if (project == null || filter == null)
      {
        return;
      }

      var updatedFilterJson = ProcessFilterJson(project.IanaTimeZone, filter.FilterJson);

      if (!string.IsNullOrEmpty(updatedFilterJson))
      {
        filter.FilterJson = updatedFilterJson;
      }
    }

    private static void GenerateIanaBasedDateTime(string ianaTimeZone, DbFilter filter)
    {
      if (string.IsNullOrEmpty(ianaTimeZone))
      {
        return;
      }

      var updatedFilterJson = ProcessFilterJson(ianaTimeZone, filter.FilterJson);

      if (!string.IsNullOrEmpty(updatedFilterJson))
      {
        filter.FilterJson = updatedFilterJson;
      }
    }

    private static string ProcessFilterJson(string ianaTimeZone, string filterJson)
    {
      MasterData.Models.Models.Filter filterObj = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filterJson);

      var utcNow = DateTime.UtcNow;

      if (filterObj.DateRangeType == null ||
          filterObj.DateRangeType == DateRangeType.ProjectExtents ||
          filterObj.DateRangeType == DateRangeType.Custom)
      {
        return string.Empty;
      }

      var startUtc = utcNow.UtcForDateRangeType((DateRangeType)filterObj.DateRangeType, ianaTimeZone, true);
      var endUtc = utcNow.UtcForDateRangeType((DateRangeType)filterObj.DateRangeType, ianaTimeZone, false);

      filterObj.SetDates(startUtc, endUtc);

      return JsonConvert.SerializeObject(filterObj);
    }
  }
}
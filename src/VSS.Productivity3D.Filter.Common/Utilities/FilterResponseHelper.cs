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

    private static void GenerateIanaBasedDateTime(string ianaTimeZone, DbFilter filter)
    {
      if (string.IsNullOrEmpty(ianaTimeZone))
      {
        return;
      }

      var utcNow = DateTime.UtcNow;
      dynamic filterObj = JsonConvert.DeserializeObject(filter.FilterJson);

      if (filterObj.dateRangeType != null &&
          filterObj.dateRangeType != DateRangeType.ProjectExtents &&
          filterObj.dateRangeType != DateRangeType.Custom)
      {
        filterObj.startUTC = utcNow.UtcForDateRangeType((DateRangeType)filterObj.dateRangeType, ianaTimeZone, true);
        filterObj.endUTC = utcNow.UtcForDateRangeType((DateRangeType)filterObj.dateRangeType, ianaTimeZone, false);

        filter.FilterJson = JsonConvert.SerializeObject(filterObj);
      }
    }
  }
}
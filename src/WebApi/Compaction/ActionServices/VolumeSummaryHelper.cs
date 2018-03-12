using System;
using System.Threading.Tasks;
using VSS.MasterData.Models.Internal;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
  /// <inheritdoc />
  public class VolumeSummaryHelper : IVolumeSummaryHelper
  {
    /// <inheritdoc />
    public RaptorConverters.VolumesType GetVolumesType(Filter filter1, Filter filter2)
    {
      if (filter1 != null && filter2 != null) // Ground to Ground
      {
        return RaptorConverters.VolumesType.Between2Filters;
      }

      if (filter1 != null) // Ground to Design
      {
        return RaptorConverters.VolumesType.BetweenFilterAndDesign;
      }

      if (filter2 != null) // Design to Ground
      {
        return RaptorConverters.VolumesType.BetweenDesignAndFilter;
      }

      return RaptorConverters.VolumesType.None;
    }

    /// <summary>
    /// Evaluates two filters for use with summary volumes ground to ground comparison.
    /// </summary>
    /// <returns></returns>
    public static bool DoGroundToGroundComparison(MasterData.Models.Models.Filter baseFilter, MasterData.Models.Models.Filter topFilter)
    {
      if (baseFilter == null || topFilter == null)
      {
        return false;
      }

      return baseFilter.DateRangeType != DateRangeType.ProjectExtents &&
             baseFilter.DateRangeType == topFilter.DateRangeType &&
             baseFilter.ElevationType == null && topFilter.ElevationType == null;
    }

    public async Task<T> WithSwallowExceptionExecute<T>(Func<Task<T>> a) where T : class
    {
      try
      {
        return await a.Invoke();
      }
      catch
      { }

      return default(T);
    }
  }
}
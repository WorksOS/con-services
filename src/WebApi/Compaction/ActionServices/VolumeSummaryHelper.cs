using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
  /// <summary>
  /// Service helper for Volume Summary APIs.
  /// </summary>
  public class VolumeSummaryHelper : IVolumeSummaryHelper
  {
    /// <summary>
    /// Get the Volumes Type for the given base and top surfaces.
    /// </summary>
    /// <param name="baseFilter">The base Filter Uid.</param>
    /// <param name="topFilter">The top Filter Uid.</param>
    /// <returns>Returns the <see cref="RaptorConverters.VolumesType"/> type for the two input surfaces.</returns>
    public RaptorConverters.VolumesType GetVolumesType(Filter baseFilter, Filter topFilter)
    {
      if (baseFilter != null && topFilter != null) // Ground to Ground
      {
        return RaptorConverters.VolumesType.Between2Filters;
      }

      if (baseFilter != null) // Ground to Design
      {
        return RaptorConverters.VolumesType.BetweenFilterAndDesign;
      }

      if (topFilter != null) // Design to Ground
      {
        return RaptorConverters.VolumesType.BetweenDesignAndFilter;
      }

      return RaptorConverters.VolumesType.None;
    }
  }
}
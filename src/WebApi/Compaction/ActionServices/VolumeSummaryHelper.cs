using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
  /// <inheritdoc />
  public class VolumeSummaryHelper : IVolumeSummaryHelper
  {
    /// <inheritdoc />
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
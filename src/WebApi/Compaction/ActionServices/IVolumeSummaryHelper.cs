using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
  /// <summary>
  /// Service helper for Volume Summary APIs.
  /// </summary>
  public interface IVolumeSummaryHelper
  {
    /// <summary>
    /// Get the Volumes Type for the given base and top surfaces.
    /// </summary>
    /// <param name="baseFilter">The base Filter Uid.</param>
    /// <param name="topFilter">The top Filter Uid.</param>
    /// <returns>Returns the <see cref="RaptorConverters.VolumesType"/> type for the two input surfaces.</returns>
    RaptorConverters.VolumesType GetVolumesType(Filter baseFilter, Filter topFilter);
  }
}
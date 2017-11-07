using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
  /// <summary>
  /// 
  /// </summary>
  public interface IVolumeSummaryHelper
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseFilter"></param>
    /// <param name="topFilter"></param>
    /// <returns></returns>
    RaptorConverters.VolumesType GetVolumesType(Filter baseFilter, Filter topFilter);
  }
}
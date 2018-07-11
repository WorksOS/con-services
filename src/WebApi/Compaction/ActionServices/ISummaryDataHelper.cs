using System;
using System.Threading.Tasks;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Compaction.ActionServices
{
  /// <summary>
  /// Service helper for Volume Summary APIs.
  /// </summary>
  public interface ISummaryDataHelper
  {
    /// <summary>
    /// Get the Volumes Type for the given base and top surfaces.
    /// </summary>
    /// <param name="filter1">Filter to compare against filter2</param>
    /// <param name="filter2">Filter to compare against filter1</param>
    /// <returns>Returns the <see cref="VolumesType"/> type for the two input surfaces.</returns>
    VolumesType GetVolumesType(FilterResult filter1, FilterResult filter2);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a"></param>
    /// <returns></returns>
    Task<T> WithSwallowExceptionExecute<T>(Func<Task<T>> a) where T : class;
  }
}

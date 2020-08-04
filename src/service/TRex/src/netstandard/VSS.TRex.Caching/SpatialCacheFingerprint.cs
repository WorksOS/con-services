using System;
using System.Text;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Caching
{
  public static class SpatialCacheFingerprint
  {
    /// <summary>
    /// Constructs a string representing the fingerprint of a request required spatial data. The fingerprint is designed to identify a context within
    /// which cached general sub grid results may be stored and accessed during query processing.
    /// </summary>
    /// <param name="projectUid">The Guid identifying the project within the fingerprint</param>
    /// <param name="gridDataType">The type of data being requested in the query</param>
    /// <param name="filter">The set of filters that govern the query</param>
    /// <param name="includedSurveyedSurfaces">The list of surveyed surfaces included in the query, expressed as a an array of GUIDs</param>
    /// <returns>A string representing the uniqueness of this spatial cache element</returns>
    public static string ConstructFingerprint(Guid projectUid, GridDataType gridDataType, ICombinedFilter filter, Guid[] includedSurveyedSurfaces)
    {
      var sb = new StringBuilder();
      sb.Append(projectUid).Append('-').Append(gridDataType);

      if (filter != null)
        sb.Append('-').Append(filter.AttributeFilter.SpatialCacheFingerprint());

      if (includedSurveyedSurfaces.Length > 0)
        includedSurveyedSurfaces.ForEach(x => sb.Append(x));

      return sb.ToString();
    }
  }
}

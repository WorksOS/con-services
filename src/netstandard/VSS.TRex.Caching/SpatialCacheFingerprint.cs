using System;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Caching
{
  public static class SpatialCacheFingerprint
  {
    /// <summary>
    /// Constructs a string representing the fingerprint of a request required spatial data. The fingerprint is designed to identify a context within
    /// which cached general subgrid results may be stored and accessed during query processing.
    /// </summary>
    /// <param name="projectUID">The Guid identifying the project within the fingerprint</param>
    /// <param name="gridDataType">The type of data being requested in the query</param>
    /// <param name="filters">The set of filters that govern the query</param>
    /// <param name="includedSurveyedSurfaces">The list of surveyed surfaces included in the query, expressed as a an array of GUIDs</param>
    /// <returns></returns>
    public static string ConstructFingerprint(Guid projectUID, GridDataType gridDataType, IFilterSet filters, Guid[] includedSurveyedSurfaces)
    {
      string fingerprint = $"{projectUID}-{gridDataType}";

      if (filters?.Filters != null && filters.Filters.Length > 0)
        foreach (var filter in filters.Filters)
          fingerprint = fingerprint + filter.AttributeFilter.SpatialCacheFingerprint();

      if (includedSurveyedSurfaces.Length > 0)
        foreach (var guid in includedSurveyedSurfaces)
          fingerprint = fingerprint + $"-{guid}";

      return fingerprint;
    }
  }
}

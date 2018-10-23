using System;
using VSS.TRex.Types;

namespace VSS.TRex.Caching
{
  public static class SpatialCacheFingerprint
  {
    /// <summary>
    /// Constructs a string representing the fingerprint of a request required spatial data. The fingerprint is designed to identify a context within
    /// which cached general subgrid results 
    /// </summary>
    /// <param name="ProjectUID"></param>
    /// <param name="gridDataType"></param>
    /// <param name="filterFingerprint"></param>
    /// <param name="surveyedSurfaceCount"></param>
    /// <param name="excludedSurveyedSurfaces"></param>
    /// <returns></returns>
    public static string ConstructFingerprint(Guid ProjectUID, GridDataType gridDataType, string filterFingerprint, int surveyedSurfaceCount, Guid[] excludedSurveyedSurfaces)
    {
      return "";
    }
  }
}

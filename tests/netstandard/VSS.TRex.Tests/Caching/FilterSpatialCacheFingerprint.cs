using VSS.TRex.Filters;
using Xunit;

namespace VSS.TRex.Tests.Caching
{
  public class FilterSpatialCacheFingerprint
  {
    [Fact]
    public void Test_GetCacheFingerPrint_Default()
    {
      string fp = new CombinedFilter().AttributeFilter.SpatialCacheFingerprint();

      Assert.True(string.IsNullOrEmpty(fp), $"Fingerprint for null filter was not empty, = '{fp}'");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludeSurveyedSurfaces()
    {
      string fp = new CombinedFilter().AttributeFilter.SpatialCacheFingerprint();

      Assert.True(fp == "ESS:1", $"Fingerprint for surveyed surface exclusion is incorrect, = '{fp}'");
    }
  }
}

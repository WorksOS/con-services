using System;
using VSS.TRex.Filters;
using Xunit;

namespace VSS.TRex.Tests.Caching
{
  public class FilterSpatialCacheFingerprint
  {
    /// <summary>
    ///  Handy helper function to make a configured filter
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    private CombinedFilter MakeFilterWith(Action<CombinedFilter> configure)
    {
      var combinedFilter = new CombinedFilter();
      configure(combinedFilter);
      return combinedFilter;
    }
    [Fact]
    public void Test_GetCacheFingerPrint_Default()
    {
      string fp = new CombinedFilter().AttributeFilter.SpatialCacheFingerprint();

      Assert.True(string.IsNullOrEmpty(fp), $"Fingerprint for null filter was not empty, = '{fp}'");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_IncludeEarliestCellPass_Present()
    {
      var filter = MakeFilterWith(x => x.AttributeFilter.ReturnEarliestFilteredCellPass = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("REFCP:1", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain earliest filtered cell pass ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_IncludeEarliestCellPass_NotPresent()
    {
      var filter = MakeFilterWith(x => x.AttributeFilter.ReturnEarliestFilteredCellPass = false);

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("REFCP", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains earliest filtered cell pass ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_RestrictFilteredDataToCompactorsOnly_Present()
    {
      var filter = MakeFilterWith(x => x.AttributeFilter.RestrictFilteredDataToCompactorsOnly = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("RFDTCO:1", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain compactor restriction ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_RestrictFilteredDataToCompactorsOnly_NotPresent()
    {
      var filter = MakeFilterWith(x => x.AttributeFilter.RestrictFilteredDataToCompactorsOnly = false);

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("RFDTCO", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains compactor restriction ID");
    }

    private const string ExcludeSurveyedSurfacesID = "ESS:1";

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasDesignFilter()
    {
      var filter = MakeFilterWith(x => x.AttributeFilter.HasDesignFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasMachineFilter()
    {
      var filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasMachineFilter = true;
        x.AttributeFilter.MachineIDs = new short[] {0};      
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasMachineDirectionFilter()
    {
      var filter = MakeFilterWith(x => x.AttributeFilter.HasMachineDirectionFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasVibeStateFilter()
    {
      var filter = MakeFilterWith(x => x.AttributeFilter.HasVibeStateFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasCompactionMachinesOnlyFilter()
    {
      var filter = MakeFilterWith(x => x.AttributeFilter.HasCompactionMachinesOnlyFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasGPSAccuracyFilter()
    {
      var filter = MakeFilterWith(x => x.AttributeFilter.HasGPSAccuracyFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasPassTypeFilter()
    {
      var filter = MakeFilterWith(x => x.AttributeFilter.HasPassTypeFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }


    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_HasTemperatureRangeFilter()
    {
      var filter = MakeFilterWith(x => x.AttributeFilter.HasTemperatureRangeFilter = true);

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains(ExcludeSurveyedSurfacesID, StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain ExcludeSurveyedSurfaces ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ExcludesSurveyedSurfaces_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("ESS", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains ExcludeSurveyedSurfaces ID");
    }
  }
}

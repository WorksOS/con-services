using System;
using VSS.TRex.Filters;
using VSS.TRex.Types;
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

    [Fact]
    public void Test_GetCacheFingerPrint_TimeFilter_Present()
    {
      var filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasTimeFilter = true;
        x.AttributeFilter.StartTime = new DateTime(1111);
        x.AttributeFilter.EndTime = new DateTime(2222);
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("TF:1111-2222", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain time filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_TimeFilter_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("TF:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains time filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_DesignFilter_Present()
    {
      var filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasDesignFilter = true;
        x.AttributeFilter.DesignNameID = 123;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("DF:123", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain design name filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_DesignFilter_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("DF:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains design name filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_MachineFilter_Present()
    {
      var filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasMachineFilter = true;
        x.AttributeFilter.MachineIDs = new short[] {1, 12, 23};
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("MF:-1-12-23", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain machine filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_MachineFilter_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("MF:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains machine filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_MachineDirection_Present()
    {
      var filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasMachineDirectionFilter = true;
        x.AttributeFilter.MachineDirection = MachineDirection.Forward;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("MD:Forward", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain machine direction filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_MachineDirection_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("MD:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains machine direction filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_VibeState_Present()
    {
      var filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasVibeStateFilter = true;
        x.AttributeFilter.VibeState = VibrationState.On;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("VS:On", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain vibe state filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_VibeState_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("VS:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains vibe state ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_MinElevMapping_Present()
    {
      var filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasMinElevMappingFilter = true;
        x.AttributeFilter.MinElevationMapping = true;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("MEM:1", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain min elevation mapping filter ID");

      filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasMinElevMappingFilter = true;
        x.AttributeFilter.MinElevationMapping = false;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("MEM:0", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain min elevation mapping filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_MinElevMapping_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("MEM:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains vibe state ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ElevationType_Present()
    {
      var filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasElevationTypeFilter = true;
        x.AttributeFilter.ElevationType = ElevationType.Last;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("ET:Last", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain elevation type filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_ElevationType_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("ET:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains elevation type filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_GuidanceMode_Present()
    {
      var filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasGCSGuidanceModeFilter = true;
        x.AttributeFilter.GCSGuidanceMode = MachineAutomaticsMode.Manual;
      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("GM:Manual", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain guidance mode filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_GuidanceMode_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("ET:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains guidance mode filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_GPSAccuracy_Present()
    {
      var filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasGPSAccuracyFilter = true;
        x.AttributeFilter.GPSAccuracy = GPSAccuracy.Fine;
        x.AttributeFilter.GPSAccuracyIsInclusive = true;

      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("GA:1-Fine", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain guidance mode filter ID");

      filter = MakeFilterWith(x =>
      {
        x.AttributeFilter.HasGPSAccuracyFilter = true;
        x.AttributeFilter.GPSAccuracy = GPSAccuracy.Fine;
        x.AttributeFilter.GPSAccuracyIsInclusive = false;

      });

      Assert.True(filter.AttributeFilter.SpatialCacheFingerprint().Contains("GA:0-Fine", StringComparison.OrdinalIgnoreCase),
        "Fingerprint does not contain GPS Accuracy mode filter ID");
    }

    [Fact]
    public void Test_GetCacheFingerPrint_GPSAccuracy_NotPresent()
    {
      var filter = new CombinedFilter();

      Assert.False(filter.AttributeFilter.SpatialCacheFingerprint().Contains("GA:", StringComparison.OrdinalIgnoreCase),
        "Fingerprint contains GPS Accuracy filter ID");
    }
  }
}

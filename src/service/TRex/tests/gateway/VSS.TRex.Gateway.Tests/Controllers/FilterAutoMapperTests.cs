using System;
using System.Collections.Generic;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers
{
  public class FilterAutoMapperTests : IClassFixture<DITagFileFixture>
  {
    [Fact]
    public void MapFilterResultWithPolygonToCombinedFilter()
    {
      var polygonLonLat = new List<WGSPoint>
      {
        new WGSPoint(1, 1),
        new WGSPoint(2, 2),
        new WGSPoint(3, 3)
      };
      var filter = new FilterResult(null, new Filter(), polygonLonLat, null, null, null, true, null);
      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filter);
      combinedFilter.AttributeFilter.Should().NotBeNull();
      combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass.Should().BeTrue();
      combinedFilter.AttributeFilter.ElevationType.Should().Be(Types.ElevationType.First);

      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList.Should().NotBeNull();
      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList.Should().BeEmpty();

      combinedFilter.SpatialFilter.Should().NotBeNull();
      combinedFilter.SpatialFilter.CoordsAreGrid.Should().BeFalse();
      combinedFilter.SpatialFilter.IsSpatial.Should().BeTrue();
      combinedFilter.SpatialFilter.Fence.Should().NotBeNull();
      combinedFilter.SpatialFilter.Fence.Points.Should().NotBeNull();
      combinedFilter.SpatialFilter.Fence.Points.Count.Should().Be(polygonLonLat.Count);
      for (int i = 0; i < combinedFilter.SpatialFilter.Fence.Points.Count; i++)
      {
        combinedFilter.SpatialFilter.Fence.Points[i].X.Should().Be(filter.PolygonLL[i].Lon);
        combinedFilter.SpatialFilter.Fence.Points[i].Y.Should().Be(filter.PolygonLL[i].Lat);
      }
    }

    [Fact]
    public void MapFilterResultNoPolygonToCombinedFilter()
    {
      var filter = new FilterResult(null, new Filter(), null, null, null, null, true, null);
      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filter);
      combinedFilter.AttributeFilter.Should().NotBeNull();
      combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass.Should().BeTrue();
      combinedFilter.AttributeFilter.ElevationType.Should().Be(Types.ElevationType.First);

      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList.Should().NotBeNull();
      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList.Should().BeEmpty();

      combinedFilter.SpatialFilter.Should().NotBeNull();
      combinedFilter.SpatialFilter.CoordsAreGrid.Should().BeFalse();
      combinedFilter.SpatialFilter.IsSpatial.Should().BeFalse();
      combinedFilter.SpatialFilter.Fence.Should().BeNull();
    }

    [Fact]
    public void MapFilterResultHasNoFiltersToCombinedFilter()
    {
      var filter = Filter.CreateFilter(null, null,
        string.Empty, string.Empty,
        new List<MachineDetails>(0), null, null, 
        null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, 
        null, null, null, null);
      
      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.Should().NotBeNull();

      combinedFilter.AttributeFilter.HasTimeFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasMachineFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasMachineDirectionFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasDesignFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasVibeStateFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasLayerStateFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasElevationMappingModeFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasElevationTypeFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasGCSGuidanceModeFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasGPSAccuracyFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasGPSToleranceFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasPositioningTechFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasLayerIDFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasElevationRangeFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasPassTypeFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasCompactionMachinesOnlyFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasTemperatureRangeFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.HasPassCountRangeFilter.Should().BeFalse();
    }

    [Fact]
    public void MapFilterResultHasTimeFilterToCombinedFilter()
    {
      var filter = Filter.CreateFilter(
        new DateTime(2018, 1, 10), new DateTime(2019, 2, 11), null, null,
        new List<MachineDetails>(0), null, null, 
        null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, 
        null, null, true, null);
      
      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.Should().NotBeNull();

      combinedFilter.AttributeFilter.HasTimeFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.StartTime.Should().Be((DateTime) (filter.StartUtc ?? null));
      combinedFilter.AttributeFilter.EndTime.Should().Be((DateTime) (filter.EndUtc ?? null));
    }

    [Fact]
    public void MapFilterResultHasMachineFilterToCombinedFilter()
    {
      var contributingMachines = new List<MachineDetails>()
      {
        new MachineDetails(Consts.NULL_LEGACY_ASSETID, "Big yella 1", false, Guid.NewGuid()),
        new MachineDetails(Consts.NULL_LEGACY_ASSETID, "Big yella 2", true, Guid.NewGuid())
      };
      var filter = Filter.CreateFilter(
        null, null, null, null,
        contributingMachines, null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null,
        null, null, true, null);


      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasMachineFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.MachinesList.Length.Should().Be(2);
      combinedFilter.AttributeFilter.MachinesList[0].Should().Be((Guid) (contributingMachines[0].AssetUid ?? null));
      combinedFilter.AttributeFilter.MachinesList[1].Should().Be((Guid) (contributingMachines[1].AssetUid ?? null));

      var internalMachineIdBitArray = combinedFilter.AttributeFilter.GetMachineIDsSet();
      internalMachineIdBitArray.Should().NotBeNull();
      internalMachineIdBitArray.Count.Should().Be(0);
    }

    [Fact]
    public void MapFilterResultHasMachineFilterToCombinedFilter_WithMachines()
    {
      var contributingMachines = new List<MachineDetails>()
      {
        new MachineDetails(Consts.NULL_LEGACY_ASSETID, "Big yella 1", false, Guid.NewGuid()),
        new MachineDetails(Consts.NULL_LEGACY_ASSETID, "Big yella 2", true, Guid.NewGuid())
      };
      var filter = Filter.CreateFilter(
        null, null, null, null,
        contributingMachines, null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null,
        null, null, true, null);


      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasMachineFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.MachinesList.Length.Should().Be(2);
      combinedFilter.AttributeFilter.MachinesList[0].Should().Be((Guid)(contributingMachines[0].AssetUid ?? null));
      combinedFilter.AttributeFilter.MachinesList[1].Should().Be((Guid)(contributingMachines[1].AssetUid ?? null));

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      siteModel.Machines.CreateNew("Test Machine 3", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, contributingMachines[0].AssetUid.Value);

      combinedFilter.AttributeFilter.SiteModel = siteModel;
      var internalMachineIdBitArray = combinedFilter.AttributeFilter.GetMachineIDsSet();
      internalMachineIdBitArray.Should().NotBeNull();
      internalMachineIdBitArray.Count.Should().Be(3);
      internalMachineIdBitArray[0].Should().BeFalse();
      internalMachineIdBitArray[1].Should().BeFalse();
      internalMachineIdBitArray[2].Should().BeTrue();
    }

    [Fact]
    public void MapFilterResultHasMachineDirectionFilterToCombinedFilter()
    {
      var filter = Filter.CreateFilter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null, 
        null, null, true, null
      );
      var filterResult = new FilterResult(null, filter, null, null,
        null, null, true, null);

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasMachineDirectionFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.MachineDirection.Should().Be(Types.MachineDirection.Forward);

      filterResult.ForwardDirection = false;
      combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasMachineDirectionFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.MachineDirection.Should().Be(Types.MachineDirection.Reverse);

      filterResult.ForwardDirection = null;
      combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasMachineDirectionFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.MachineDirection.Should().Be(Types.MachineDirection.Unknown);
    }

    [Fact]
    public void MapFilterResultHasDesignFilterToCombinedFilter()
    {
      var onMachineDesignId = 66;
      var filter = Filter.CreateFilter(
        null, null, null, null,
        new List<MachineDetails>(0), onMachineDesignId, null, 
        null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null,
        null, null, true, null);
      
      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasDesignFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.DesignNameID.Should().Be(onMachineDesignId);
    }

    [Fact]
    public void MapFilterResultHasLayerStateFilterToCombinedFilter()
    {
      // todoJeannie Raymond... how is HasLayerStateFilter determined
      throw new NotImplementedException();
    }

    [Fact]
    public void MapFilterResultHasElevationMappingModeFilterToCombinedFilter()
    {
      // todoJeannie Raymond... how is ElevationMappingMode determined
      throw new NotImplementedException();
    }

    [Fact]
    public void MapFilterResultHasElevationTypeFilterToCombinedFilter()
    {
      var filterResult = new FilterResult(null, new Filter(), null, null, 
        null, null, true, null);
      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.Should().NotBeNull();
      combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass.Should().BeTrue();
      combinedFilter.AttributeFilter.HasElevationTypeFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.ElevationType.Should().Be(Types.ElevationType.First);

      filterResult.ReturnEarliest = false;
      combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass.Should().BeFalse();
      combinedFilter.AttributeFilter.HasElevationTypeFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.ElevationType.Should().Be(Types.ElevationType.Last);

      filterResult.ReturnEarliest = null;
      combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass.Should().BeFalse();
      combinedFilter.AttributeFilter.HasElevationTypeFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.ElevationType.Should().Be(Types.ElevationType.Last);
    }

    [Fact]
    public void MapFilterResultHasGCSGuidanceModeFilterToCombinedFilter()
    {
      // todoJeannie Raymond... how is GCSGuidanceMode determined
      throw new NotImplementedException();
    }

    [Fact]
    public void MapFilterResultHasGPSAccuracyFilterToCombinedFilter()
    {
      // todoJeannie Raymond... how is GPSAccuracy determined
      throw new NotImplementedException();
    }

    [Fact]
    public void MapFilterResultHasGPSToleranceFilterToCombinedFilter()
    {
      // todoJeannie Raymond... how is GPSTolerance determined
      throw new NotImplementedException();
    }

    [Fact]
    public void MapFilterResultHasPositioningTechFilterToCombinedFilter()
    {
      // todoJeannie Raymond... how is PositioningTech determined
      throw new NotImplementedException();
    }

    [Fact]
    public void MapFilterResultHasLayerIdFilterToCombinedFilter()
    {
      var filter = Filter.CreateFilter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null,
        null, null, null, 45
      );
      var filterResult = new FilterResult(null, filter, null, null,
        null, null, true, null);

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasLayerIDFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.LayerID.Should().Be(45);
    }

    [Fact]
    public void MapFilterResultHasElevationRangeFilterToCombinedFilter()
    {
      // todoJeannie Raymond... how is ElevationRange determined
      throw new NotImplementedException();
    }

    [Fact]
    public void MapFilterResultHasPassTypeFilterToCombinedFilter()
    {
      // todoJeannie Raymond... how is PassType determined
      throw new NotImplementedException();
    }

    [Fact]
    public void MapFilterResultHasCompactionMachinesOnlyFilterToCombinedFilter()
    {
      // todoJeannie Raymond... how is CompactionMachinesOnly determined
      throw new NotImplementedException();
    }

    [Fact]
    public void MapFilterResultHasTemperatureRangeFilterToCombinedFilter()
    {
      var filter = Filter.CreateFilter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null,
        null, null, null, null,
        temperatureRangeMin: 34, temperatureRangeMax: 89
      );
      var filterResult = new FilterResult(null, filter, null, null,
        null, null, true, null);

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasTemperatureRangeFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.MaterialTemperatureMin.Should().Be(34);
      combinedFilter.AttributeFilter.MaterialTemperatureMax.Should().Be(89);
    }

    [Fact]
    public void MapFilterResultHasPassCountRangeFilterToCombinedFilter()
    {
      var filter = Filter.CreateFilter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null,
        null, null, null, null,
        passCountRangeMin: 34, passCountRangeMax: 89
      );
      var filterResult = new FilterResult(null, filter, null, null,
        null, null, true, null);

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasPassCountRangeFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.PassCountRangeMin.Should().Be(34);
      combinedFilter.AttributeFilter.PassCountRangeMax.Should().Be(89);
    }
  }
}

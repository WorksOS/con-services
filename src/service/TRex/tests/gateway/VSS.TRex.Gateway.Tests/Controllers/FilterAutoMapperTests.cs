using System;
using System.Collections.Generic;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;
using ElevationType = VSS.TRex.Common.Types.ElevationType;

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
      var filterResult = new FilterResult(null, new Filter(), polygonLonLat, null, null, null, null, true, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.Should().NotBeNull();
      combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass.Should().BeTrue();

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
        combinedFilter.SpatialFilter.Fence.Points[i].X.Should().Be(filterResult.PolygonLL[i].Lon);
        combinedFilter.SpatialFilter.Fence.Points[i].Y.Should().Be(filterResult.PolygonLL[i].Lat);
      }
    }

    [Fact]
    public void MapFilterResultNoPolygonToCombinedFilter()
    {
      var filterResult = new FilterResult(null, new Filter(), null, null, null, null, null, true, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.Should().NotBeNull();
      combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass.Should().BeTrue();

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
      var filter = new Filter(null, null,
        string.Empty, string.Empty,
        new List<MachineDetails>(0), null, null,
        null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null,
        null, null, null, null);
      filterResult.Validate();

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
      var filter = new Filter(
        DateTime.SpecifyKind(new DateTime(2018, 1, 10), DateTimeKind.Utc),
        DateTime.SpecifyKind(new DateTime(2019, 2, 11), DateTimeKind.Utc),
        null, null,
        new List<MachineDetails>(0), null, null, 
        null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null,
        null, null, true, null);
      filterResult.Validate();

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
      var filter = new Filter(
        null, null, null, null,
        contributingMachines, null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null,
        null, null, true, null);
      filterResult.Validate();

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
      var filter = new Filter(
        null, null, null, null,
        contributingMachines, null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null,
        null, null, true, null);
      filterResult.Validate();

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
      var filter = new Filter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null, 
        null, null, true, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null,
        null, null, true, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasMachineDirectionFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.MachineDirection.Should().Be(MachineDirection.Forward);

      filter = new Filter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null,
        null, null, false, null
      );
      filterResult = new FilterResult(null, filter, null, null, null,
        null, null, true, null);
      combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasMachineDirectionFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.MachineDirection.Should().Be(MachineDirection.Reverse);

      filter = new Filter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null,
        null, null, null, null
      );
      filterResult = new FilterResult(null, filter, null, null, null,
        null, null, true, null);
      combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasMachineDirectionFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.MachineDirection.Should().Be(MachineDirection.Unknown);
    }

    [Fact]
    public void MapFilterResultHasMachineDesignFilterToCombinedFilter()
    {
      var onMachineDesignId = 66;
      var filter = new Filter(
        null, null, null, null,
        new List<MachineDetails>(0), onMachineDesignId, null, 
        null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null,
        null, null, true, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasDesignFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.DesignNameID.Should().Be(onMachineDesignId);
    }

    [Fact]
    public void MapFilterResultHasVibeStateFilterToCombinedFilter()
    {
      var filter = new Filter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null,
        true, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null,
        null, null, null, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasVibeStateFilter.Should().BeTrue();
    }

    [Fact]
    public void MapFilterResultHasLayerStateFilterToCombinedFilter()
    {
      FilterLayerMethod? layerType = FilterLayerMethod.Automatic;
      var filter = new Filter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null,
        null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, 
        layerType, null, null, true, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasLayerStateFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.LayerState.Should().Be(LayerState.On);

      layerType = null;
      filterResult = new FilterResult(null, filter, null, null, 
        layerType, null, null, true, null);
      filterResult.Validate();

      combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasLayerStateFilter.Should().BeFalse();
      combinedFilter.AttributeFilter.LayerState.Should().Be(LayerState.Off);
    }

    [Fact]
    public void MapFilterResultHasElevationTypeFilterToCombinedFilter()
    {
      var filter = new Filter(null, null, null, null, null, null, MasterData.Models.Models.ElevationType.First, null, null,null,null);
      var filterResult = new FilterResult(null, filter, null, null, null,
        null, null, true, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.Should().NotBeNull();
      combinedFilter.AttributeFilter.ReturnEarliestFilteredCellPass.Should().BeTrue();
      combinedFilter.AttributeFilter.HasElevationTypeFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.ElevationType.Should().Be(ElevationType.First);
    }

    [Fact]
    public void MapFilterResultHasGCSGuidanceModeFilterToCombinedFilter()
    {
      var automaticsType = AutomaticsType.Automatics;
      var filter = new Filter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null,
        null, null, null, null, automaticsType: automaticsType
      );
      var filterResult = new FilterResult(null, filter, null, null, null, 
        null, null, true, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasGCSGuidanceModeFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.GCSGuidanceMode.Should().Be(automaticsType);
    }

    [Fact]
    public void MapFilterResultHasLayerIdFilterToCombinedFilter()
    {
      var layerType = FilterLayerMethod.TagfileLayerNumber;
      var filter = new Filter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null,
        null, null, null, 45
      );
      var filterResult = new FilterResult(null, filter, null, null,
        layerType, null, null, true, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasLayerIDFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.LayerID.Should().Be(45);
    }
    
    [Fact]
    public void MapFilterResultHasTemperatureRangeFilterToCombinedFilter()
    {
      var filter = new Filter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null,
        null, null, null, null,
        temperatureRangeMin: 34, temperatureRangeMax: 89
      );
      var filterResult = new FilterResult(null, filter, null, null, null,
        null, null, true, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasTemperatureRangeFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.MaterialTemperatureMin.Should().Be(340);
      combinedFilter.AttributeFilter.MaterialTemperatureMax.Should().Be(890);
    }

    [Fact]
    public void MapFilterResultHasPassCountRangeFilterToCombinedFilter()
    {
      var filter = new Filter(
        null, null, null, null,
        new List<MachineDetails>(0), null, null,
        null, null, null, null,
        passCountRangeMin: 34, passCountRangeMax: 89
      );
      var filterResult = new FilterResult(null, filter, null, null, null,
        null, null, true, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.HasPassCountRangeFilter.Should().BeTrue();
      combinedFilter.AttributeFilter.PassCountRangeMin.Should().Be(34);
      combinedFilter.AttributeFilter.PassCountRangeMax.Should().Be(89);
    }

    [Fact]
    public void MapFilterResultHasExcludedSurveyedSurfacesToCombinedFilter()
    {
      var id1 = Guid.NewGuid();
      var id2 = Guid.NewGuid();
      var excludedIds = new List<Guid> {id1, id2};
      var filterResult = new FilterResult(null, new Filter(), null, null, null,
        null, excludedIds, true, null);
      filterResult.Validate();

      var combinedFilter = AutoMapperUtility.Automapper.Map<CombinedFilter>(filterResult);
      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList.Should().NotBeNull();
      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList.Length.Should().Be(excludedIds.Count);
      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList[0].Should().Be(id1);
      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList[1].Should().Be(id2);
    }
  }
}

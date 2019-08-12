using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Types;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Types;
using static VSS.TRex.Gateway.Common.Converters.AutoMapperUtility;
using ElevationType = VSS.TRex.Common.Types.ElevationType;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class CombinedFilterProfile : Profile
  {
    public class CustomCellPassAttributeFilterResolver : IValueResolver<FilterResult, CombinedFilter, ICellPassAttributeFilter>
    {
      public ICellPassAttributeFilter Resolve(FilterResult src, CombinedFilter dst, ICellPassAttributeFilter member, ResolutionContext context)
      {
        var filter = new CellPassAttributeFilter();

        if (src == null)
          return filter;
        
        if (src.StartUtc.HasValue)
        {
          filter.StartTime = src.StartUtc.Value;
          filter.HasTimeFilter = true;
        }

        if (src.EndUtc.HasValue)
        {
          filter.EndTime = src.EndUtc.Value;
          filter.HasTimeFilter = true;
        }

        //src.OnMachineDesignName - Can't do this in TRex at present

        filter.HasDesignFilter = src.OnMachineDesignId.HasValue;
        if (filter.HasDesignFilter)
        {
          filter.DesignNameID = (int) (src.OnMachineDesignId.Value);
        }

        filter.HasMachineFilter = src.ContributingMachines != null && src.ContributingMachines.Count > 0;
        if (filter.HasMachineFilter)
        {
          filter.MachinesList = src.ContributingMachines.Where(m => m.AssetUid.HasValue)
            .Select(m => m.AssetUid.Value).ToArray();
        }

        if (src.CompactorDataOnly.HasValue)
        {
          filter.HasCompactionMachinesOnlyFilter = src.CompactorDataOnly.Value;
        }

        filter.HasVibeStateFilter = src.VibeStateOn.HasValue;
        if (filter.HasVibeStateFilter)
        {
          filter.VibeState = src.VibeStateOn.Value ? VibrationState.On : VibrationState.Off;
        }

        filter.HasElevationTypeFilter = src.ElevationType.HasValue;
        if (filter.HasElevationTypeFilter)
        {
          filter.ElevationType = (ElevationType) src.ElevationType.Value;
        }

        filter.HasMachineDirectionFilter = src.ForwardDirection.HasValue;
        if (filter.HasMachineDirectionFilter)
        {
          filter.MachineDirection = src.ForwardDirection.Value
            ? MachineDirection.Forward
            : MachineDirection.Reverse;
        }

        // Layer Analysis
        filter.HasLayerStateFilter = src.LayerType.HasValue;
        if (filter.HasLayerStateFilter)
        {
          //filter.LayerMethod is used to set LiftSettings.LiftDetectionType elsewhere. LayerMethod is not in the TRex filter.
          filter.LayerState = LayerState.On;

          if (src.LayerType == FilterLayerMethod.OffsetFromDesign ||
              src.LayerType == FilterLayerMethod.OffsetFromBench ||
              src.LayerType == FilterLayerMethod.OffsetFromProfile)
          {
            if (src.LayerType == FilterLayerMethod.OffsetFromBench)
            {
              filter.ElevationRangeLevel = src.BenchElevation.HasValue ? src.BenchElevation.Value : 0;
            }
            else
            {
              filter.ElevationRangeDesign = new DesignOffset(src.LayerDesignOrAlignmentFile.FileUid.Value, src.LayerDesignOrAlignmentFile.Offset);
            }

            if (src.LayerNumber.HasValue && src.LayerThickness.HasValue)
            {
              int layerNumber = src.LayerNumber.Value < 0
                ? src.LayerNumber.Value + 1
                : src.LayerNumber.Value;
              filter.ElevationRangeOffset = layerNumber * src.LayerThickness.Value;
              filter.ElevationRangeThickness = src.LayerThickness.Value;
            }
            else
            {
              filter.ElevationRangeOffset = 0;
              filter.ElevationRangeThickness = 0;
            }

            filter.HasElevationRangeFilter = true;
          }
          else if (src.LayerType == FilterLayerMethod.TagfileLayerNumber)
          {
            filter.LayerID = src.LayerNumber.Value;
            filter.HasLayerIDFilter = true;
          }
        }
        else
        {
          filter.LayerState = LayerState.Off;
        }

        filter.HasGPSAccuracyFilter = src.GpsAccuracy.HasValue;
        if (filter.HasGPSAccuracyFilter)
        {
          filter.GPSAccuracy = (GPSAccuracy)src.GpsAccuracy;
          filter.GPSAccuracyIsInclusive = src.GpsAccuracyIsInclusive ?? false;
        }

        if (src.BladeOnGround.HasValue && src.BladeOnGround.Value)
        {
          filter.HasPassTypeFilter = true;
          filter.PassTypeSet |= PassTypeSet.Front;
          filter.PassTypeSet |= PassTypeSet.Rear;
        }

        if (src.TrackMapping.HasValue && src.TrackMapping.Value)
        {
          filter.HasPassTypeFilter = true;
          filter.PassTypeSet |= PassTypeSet.Track;
        }

        if (src.WheelTracking.HasValue && src.WheelTracking.Value)
        {
          filter.HasPassTypeFilter = true;
          filter.PassTypeSet |= PassTypeSet.Wheel;
        }

        if (src.ExcludedSurveyedSurfaceUids != null)
          filter.SurveyedSurfaceExclusionList = src.ExcludedSurveyedSurfaceUids.ToArray();

        filter.ReturnEarliestFilteredCellPass = src.ReturnEarliest.HasValue && src.ReturnEarliest.Value;

        filter.HasGCSGuidanceModeFilter = src.AutomaticsType.HasValue;
        if (filter.HasGCSGuidanceModeFilter)
        {
          filter.GCSGuidanceMode = (AutomaticsType)src.AutomaticsType.Value;
        }

        filter.HasTemperatureRangeFilter = src.TemperatureRangeMin.HasValue && src.TemperatureRangeMax.HasValue;
        if (filter.HasTemperatureRangeFilter)
        {
          filter.MaterialTemperatureMin = (ushort)(src.TemperatureRangeMin.Value * 10);
          filter.MaterialTemperatureMax = (ushort)(src.TemperatureRangeMax.Value * 10);
        }

        filter.HasPassCountRangeFilter = src.PassCountRangeMin.HasValue && src.PassCountRangeMax.HasValue;
        if (filter.HasPassCountRangeFilter)
        {
          filter.PassCountRangeMin = (ushort)src.PassCountRangeMin.Value;
          filter.PassCountRangeMax = (ushort)src.PassCountRangeMax.Value;
        }

        //These are not used?
        //HasElevationMappingModeFilter
        //HasPositioningTechFilter
        return filter;
      }
    }

    public class CustomCellSpatialFilterResolver : IValueResolver<FilterResult, CombinedFilter, ICellSpatialFilter>
    {
      public ICellSpatialFilter Resolve(FilterResult src, CombinedFilter dst, ICellSpatialFilter member, ResolutionContext context)
      {
        Fence fence = null;
        if (src.PolygonGrid != null)
        {
          fence = new Fence();
          fence.Points = Automapper.Map<List<Point>, List<FencePoint>>(src.PolygonGrid);
          fence.UpdateExtents();
        }
        else if (src.PolygonLL != null)
        {
          fence = new Fence();
          fence.Points = Automapper.Map<List<WGSPoint>, List<FencePoint>>(src.PolygonLL);
          fence.UpdateExtents();
        }

        var fileUid = src.DesignFile != null ? src.DesignFile.FileUid.Value : (src.AlignmentFile != null ? src.AlignmentFile.FileUid.Value : Guid.Empty);

        return new CellSpatialFilter
        {
          CoordsAreGrid = src.PolygonGrid != null,
          IsSpatial = fence != null,
          Fence = fence,
          IsDesignMask = src.DesignFile != null,
          IsAlignmentMask = src.AlignmentFile != null,
          AlignmentDesignMaskDesignUID = fileUid,
          StartStation = src.StartStation,
          EndStation = src.EndStation,
          LeftOffset = src.LeftOffset,
          RightOffset = src.RightOffset
        };
      }
    }

    public CombinedFilterProfile()
    {
      CreateMap<FilterResult, CombinedFilter>()
        .ForMember(x => x.AttributeFilter,
        opt => opt.ResolveUsing<CustomCellPassAttributeFilterResolver>())

      .ForMember(x => x.SpatialFilter,
          opt => opt.ResolveUsing<CustomCellSpatialFilterResolver>());
    }
   
  }
}

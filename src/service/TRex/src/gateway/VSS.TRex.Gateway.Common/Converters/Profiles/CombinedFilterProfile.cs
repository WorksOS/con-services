using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Types;
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
        var returnEarliestFilteredCellPass = src.ReturnEarliest.HasValue && src.ReturnEarliest.Value && src.ReturnEarliest.Value == true;
        
        var contributingMachines = new Guid[0];
        // todoJeannie
        //  set siteModel 
        //  should check if any machines are valid for site??
        // validate
        if (src.ContributingMachines != null && src.ContributingMachines.Count > 0)
          contributingMachines = (src.ContributingMachines.Where(m => m.AssetUid.HasValue)
          .Select(m => m.AssetUid.Value).ToArray());
        
        
        return new CellPassAttributeFilter
        {
          HasTimeFilter = src.StartUtc.HasValue && src.EndUtc.HasValue,
          StartTime = src.StartUtc ?? TRex.Common.Consts.MIN_DATETIME_AS_UTC,
          EndTime = src.EndUtc ?? TRex.Common.Consts.MAX_DATETIME_AS_UTC,

          HasMachineFilter = contributingMachines.Length > 0,
          MachinesList = contributingMachines,
         
          HasMachineDirectionFilter = src.ForwardDirection.HasValue,
          MachineDirection = src.ForwardDirection.HasValue 
                            ? ( src.ForwardDirection == true ? MachineDirection.Forward : MachineDirection.Reverse)
                            : MachineDirection.Unknown,

          HasDesignFilter = src.OnMachineDesignId.HasValue,
          DesignNameID = (int) (src.OnMachineDesignId ?? Consts.kNoDesignNameID),

          HasVibeStateFilter = src.VibeStateOn.HasValue && src.VibeStateOn.Value,
          HasLayerStateFilter = src.LayerType.HasValue,
          LayerState = src.LayerType.HasValue ? LayerState.On : LayerState.Off,

          HasElevationMappingModeFilter = false, // todoJeannie help, does this have something to do with LayerType/Number?

          HasElevationTypeFilter = src.ReturnEarliest.HasValue,
          ReturnEarliestFilteredCellPass = returnEarliestFilteredCellPass,
          ElevationType = returnEarliestFilteredCellPass ? ElevationType.First : ElevationType.Last,

          HasGCSGuidanceModeFilter = src.AutomaticsType.HasValue,
          GCSGuidanceMode = src.AutomaticsType ?? AutomaticsType.Unknown,

          HasGPSAccuracyFilter = false,     // todoJeannie this filter is not set-able in FilterResult (GPSAccuracy)
          HasGPSToleranceFilter = false,    // todoJeannie this filter is not in FilterResult (set directly on Raptor TICFilterSettings?)
          HasPositioningTechFilter = false, // todoJeannie this filter is not in FilterResult (set directly on Raptor TICFilterSettings?)

          HasLayerIDFilter = src.LayerNumber.HasValue,
          LayerID = (int) (src.LayerNumber ?? CellPassConsts.NullLayerID),

          HasElevationRangeFilter = false,  // todoJeannie help, does this have something to do with LayerType/Number?
          HasPassTypeFilter = false,        // todoJeannie this filter is not set-able in FilterResult (bladeOnGround)
          HasCompactionMachinesOnlyFilter = false, // todoJeannie this filter is not set-able in FilterResult (compactorDataOnly)

          HasTemperatureRangeFilter = src.TemperatureRangeMin.HasValue && src.TemperatureRangeMax.HasValue,
          MaterialTemperatureMin = (ushort) (src.TemperatureRangeMin ?? CellPassConsts.NullMaterialTemperatureValue),
          MaterialTemperatureMax = (ushort) (src.TemperatureRangeMax ?? CellPassConsts.NullMaterialTemperatureValue),

          HasPassCountRangeFilter = src.PassCountRangeMin.HasValue && src.PassCountRangeMax.HasValue,
          PassCountRangeMin = (ushort) (src.PassCountRangeMin ?? CellPassConsts.NullPassCountValue),
          PassCountRangeMax = (ushort) (src.PassCountRangeMax ?? CellPassConsts.NullPassCountValue),
        };
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

        return new CellSpatialFilter
        {
          CoordsAreGrid = src.PolygonGrid != null,
          IsSpatial = fence != null,
          Fence = fence
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

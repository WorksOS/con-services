using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Queues;
using VSS.TRex.Machines;
using VSS.TRex.Types;
using static VSS.TRex.Gateway.Common.Converters.AutoMapperUtility;

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
        // todoJeannie should check if any machines are valid for site??
        if (src.ContributingMachines != null && src.ContributingMachines.Count > 0)
          contributingMachines = (src.ContributingMachines.Where(m => m.AssetUid.HasValue)
          .Select(m => m.AssetUid.Value).ToArray());
        
        
        return new CellPassAttributeFilter
        {
          HasTimeFilter = src.StartUtc.HasValue && src.EndUtc.HasValue,
          StartTime = src.StartUtc ?? DateTime.MinValue,
          EndTime = src.EndUtc ?? DateTime.MaxValue,

          HasMachineFilter = contributingMachines.Length > 0,
          MachinesList = contributingMachines,
         
          HasMachineDirectionFilter = src.ForwardDirection.HasValue,
          MachineDirection = src.ForwardDirection.HasValue 
                            ? ( src.ForwardDirection == true ? MachineDirection.Forward : MachineDirection.Reverse)
                            : MachineDirection.Unknown,

          HasDesignFilter = src.OnMachineDesignId.HasValue,
          DesignNameID = (int) (src.OnMachineDesignId ?? Consts.kNoDesignNameID),

          HasElevationMappingModeFilter = false, // todoJeannie

          HasElevationTypeFilter = src.ReturnEarliest.HasValue,
          ReturnEarliestFilteredCellPass = returnEarliestFilteredCellPass,
          ElevationType = returnEarliestFilteredCellPass ? Types.ElevationType.First : Types.ElevationType.Last,

          HasGCSGuidanceModeFilter = false, // todoJeannie 
          HasGPSAccuracyFilter = false, // todoJeannie 
          HasGPSToleranceFilter = false, // todoJeannie 
          HasPositioningTechFilter = false, // todoJeannie 

          HasLayerIDFilter = src.LayerNumber.HasValue,
          LayerID = (int) (src.LayerNumber ?? CellPassConsts.NullLayerID),

          HasElevationRangeFilter = false, // todoJeannie 
          HasPassTypeFilter = false, // todoJeannie 
          HasCompactionMachinesOnlyFilter = false, // todoJeannie 

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

using System.Collections.Generic;
using AutoMapper;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using static VSS.TRex.Gateway.Common.Converters.AutoMapperUtility;

namespace VSS.TRex.Gateway.Common.Converters.Profiles
{
  public class CombinedFilterProfile : Profile
  {

    public class CustomCellPassAttributeFilterResolver : IValueResolver<FilterResult, CombinedFilter, ICellPassAttributeFilter>
    {
      public ICellPassAttributeFilter Resolve(FilterResult src, CombinedFilter dst, ICellPassAttributeFilter member, ResolutionContext context)
      {
        var returnEarliestFilteredCellPass = src.ReturnEarliest.HasValue && src.ReturnEarliest.Value;

        return new CellPassAttributeFilter
        {
          ReturnEarliestFilteredCellPass = returnEarliestFilteredCellPass,
          HasElevationTypeFilter = true,
          ElevationType = returnEarliestFilteredCellPass ? Types.ElevationType.First : Types.ElevationType.Last,
          SurveyedSurfaceExclusionList = null //done afterwards
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

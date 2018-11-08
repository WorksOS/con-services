using System.Collections.Generic;
using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.MasterData.Models.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.Gateway.Common.Converters
{
  public class AutoMapperUtility
  {
    private static MapperConfiguration _automapperConfiguration;

    public static MapperConfiguration AutomapperConfiguration
    {
      get
      {
        if (_automapperConfiguration == null)
        {
          ConfigureAutomapper();
        }

        return _automapperConfiguration;
      }
    }

    private static IMapper _automapper;

    public static IMapper Automapper
    {
      get
      {
        if (_automapperConfiguration == null)
        {
          ConfigureAutomapper();
        }

        return _automapper;
      }
    }


    public static void ConfigureAutomapper()
    {

      _automapperConfiguration = new MapperConfiguration(
        //define mappings <source type, destination type>
        cfg =>
        {
          cfg.AllowNullCollections = true; // so that byte[] can be null
          cfg.AddProfile<BoundingWorldExtent3DProfile>();
          cfg.AddProfile<FenceProfile>();
          cfg.AddProfile<CombinedFilterProfile>();
        }
      );

      _automapper = _automapperConfiguration.CreateMapper();

    }


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
        }
        else if (src.PolygonLL != null)
        {
          fence = new Fence();
          fence.Points = Automapper.Map<List<WGSPoint3D>, List<FencePoint>>(src.PolygonLL);
        }

        return new CellSpatialFilter
        {
          CoordsAreGrid = src.PolygonGrid != null,
          IsSpatial = fence != null,
          Fence = fence
        };
      }
    }

    public class FenceProfile : Profile
    {
      public FenceProfile()
      {
        CreateMap<Point, FencePoint>()
          .ForMember(x => x.X,
            opt => opt.MapFrom(f => f.x))
          .ForMember(x => x.Y,
            opt => opt.MapFrom(f => f.y))
          .ForMember(x => x.Z,
            opt => opt.UseValue(0));

        CreateMap<WGSPoint3D, FencePoint>()
          .ForMember(x => x.X,
            opt => opt.MapFrom(f => f.Lon))
          .ForMember(x => x.Y,
            opt => opt.MapFrom(f => f.Lat))
          .ForMember(x => x.Z,
            opt => opt.UseValue(0));
      }
    }

    public class BoundingWorldExtent3DProfile : Profile
    {
      public BoundingWorldExtent3DProfile()
      {
        CreateMap<BoundingBox2DGrid, BoundingWorldExtent3D>()
          .ForMember(x => x.MinX,
            opt => opt.MapFrom(f => f.BottomLeftX))
          .ForMember(x => x.MinY,
            opt => opt.MapFrom(f => f.BottomleftY))
          .ForMember(x => x.MinZ,
            opt => opt.UseValue(0))
          .ForMember(x => x.MaxX,
            opt => opt.MapFrom(f => f.TopRightX))
          .ForMember(x => x.MaxY,
            opt => opt.MapFrom(f => f.TopRightY))
          .ForMember(x => x.MaxZ,
            opt => opt.UseValue(0));

        CreateMap<BoundingBox2DLatLon, BoundingWorldExtent3D>()
          .ForMember(x => x.MinX,
            opt => opt.MapFrom(f => f.BottomLeftLon))
          .ForMember(x => x.MinY,
            opt => opt.MapFrom(f => f.BottomLeftLat))
          .ForMember(x => x.MinZ,
            opt => opt.UseValue(0))
          .ForMember(x => x.MaxX,
            opt => opt.MapFrom(f => f.TopRightLon))
          .ForMember(x => x.MaxY,
            opt => opt.MapFrom(f => f.TopRightLat))
          .ForMember(x => x.MaxZ,
            opt => opt.UseValue(0));
      }
    }


    public class CombinedFilterProfile : Profile
    {
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
}

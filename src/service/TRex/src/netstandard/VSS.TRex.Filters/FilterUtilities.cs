using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Filters
{
  public static class FilterUtilities
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(nameof(FilterUtilities));

    /*
    private IExistenceMaps existenceMaps = null;
    private IExistenceMaps GetExistenceMaps() => existenceMaps ?? (existenceMaps = DIContext.Obtain<IExistenceMaps>());
    */

    /// <summary>
    /// Prepare a filter for use by performing any necessary coordinate conversions and requesting any
    /// supplemental information such as alignment design boundary calculations.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="dataModelId"></param>
    /// <returns></returns>
    public static async Task<RequestErrorStatus> PrepareFilterForUse(ICombinedFilter filter, Guid dataModelId)
    {
      // Fence DesignBoundary = null;
      var result = RequestErrorStatus.OK;

      //RequestResult: TDesignProfilerRequestResult;

      if (filter == null)
        return result;

      if (filter.SpatialFilter != null)
      {
        if (!filter.SpatialFilter.CoordsAreGrid)
        {
          var SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(dataModelId);

          XYZ[] LLHCoords;
          // If the filter has a spatial or positional context, then convert the LLH values in the
          // spatial context into the NEE values consistent with the data model.
          if (filter.SpatialFilter.IsSpatial)
          {
            LLHCoords = new XYZ[filter.SpatialFilter.Fence.NumVertices];

            // Note: Lat/Lons in filter fence boundaries are supplied to us in decimal degrees, not radians
            for (var FencePointIdx = 0; FencePointIdx < filter.SpatialFilter.Fence.NumVertices; FencePointIdx++)
            {
              LLHCoords[FencePointIdx] = new XYZ(MathUtilities.DegreesToRadians(filter.SpatialFilter.Fence[FencePointIdx].X), MathUtilities.DegreesToRadians(filter.SpatialFilter.Fence[FencePointIdx].Y));
            }

            var (errorCode, NEECoords) = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(SiteModel.CSIB(), LLHCoords);

            if (errorCode != RequestErrorStatus.OK)
            {
              Log.LogInformation("Summary volume failure, could not convert coordinates from WGS to grid coordinates");

              return RequestErrorStatus.FailedToConvertClientWGSCoords;
            }

            for (var fencePointIdx = 0; fencePointIdx < filter.SpatialFilter.Fence.NumVertices; fencePointIdx++)
            {
              filter.SpatialFilter.Fence[fencePointIdx].X = NEECoords[fencePointIdx].X;
              filter.SpatialFilter.Fence[fencePointIdx].Y = NEECoords[fencePointIdx].Y;
            }

            // Ensure that the bounding rectangle for the filter fence correctly encloses the newly calculated grid coordinates
            filter.SpatialFilter.Fence.UpdateExtents();
          }

          if (filter.SpatialFilter.IsPositional)
          {
            // Note: Lat/Lons in positions are supplied to us in decimal degrees, not radians
            LLHCoords = new[] {new XYZ(MathUtilities.DegreesToRadians(filter.SpatialFilter.PositionX), MathUtilities.DegreesToRadians(filter.SpatialFilter.PositionY))};

            var (errorCode, NEECoords) = await DIContext.Obtain<IConvertCoordinates>().LLHToNEE(SiteModel.CSIB(), LLHCoords);

            if (errorCode != RequestErrorStatus.OK)
            {
              Log.LogInformation("Filter mutation failure, could not convert coordinates from WGS to grid coordinates");

              return RequestErrorStatus.FailedToConvertClientWGSCoords;
            }

            filter.SpatialFilter.PositionX = NEECoords[0].X;
            filter.SpatialFilter.PositionY = NEECoords[0].Y;
          }

          filter.SpatialFilter.CoordsAreGrid = true;
        }

        // Ensure that the bounding rectangle for the filter fence correctly encloses the newly calculated grid coordinates
        filter.SpatialFilter?.Fence.UpdateExtents();

        // Is there an alignment file to look up? If so, only do it if there not an already existing alignment fence boundary
        if (filter.SpatialFilter.HasAlignmentDesignMask() && !(filter.SpatialFilter.AlignmentFence?.HasVertices ?? true))
        {
          var boundaryRequest = new AlignmentDesignFilterBoundaryRequest();

          var BoundaryResult = boundaryRequest.Execute
          (new AlignmentDesignFilterBoundaryArgument()
          {
            ReferenceDesign = new DesignOffset(filter.SpatialFilter.AlignmentDesignMaskDesignUID, 0),
            StartStation = filter.SpatialFilter.StartStation ?? Common.Consts.NullDouble,
            EndStation = filter.SpatialFilter.EndStation ?? Common.Consts.NullDouble,
            LeftOffset = filter.SpatialFilter.LeftOffset ?? Common.Consts.NullDouble,
            RightOffset = filter.SpatialFilter.RightOffset ?? Common.Consts.NullDouble
          });

          if (BoundaryResult.RequestResult != DesignProfilerRequestResult.OK)
          {
            Log.LogError($"{nameof(PrepareFilterForUse)}: Failed to get boundary for alignment design ID:{filter.SpatialFilter.AlignmentDesignMaskDesignUID}");

            return RequestErrorStatus.NoResultReturned;
          }

          filter.SpatialFilter.AlignmentFence = BoundaryResult.Boundary;
        }

        // Is there a surface design to look up
        if (filter.SpatialFilter.HasSurfaceDesignMask())
        {
          // Todo: Not yet supported (or demonstrated that it's needed as this should be taken care of in the larger scale determination of the sub grids that need to be queried).

          /* If the filter needs to retain a reference to the existence map, then do this...
          Filter.SpatialFilter.DesignMaskExistenceMap = GetExistenceMaps().GetSingleExistenceMap(ProjectUid, EXISTENCE_MAP_DESIGN_DESCRIPTOR, Filter.SpatialFilter.SurfaceDesignMaskDesignUid);

          if (Filter.SpatialFilter.DesignMaskExistenceMap == null)
          {
            Log.LogError($"PrepareFilterForUse: Failed to get existence map for surface design ID:{Filter.SpatialFilter.SurfaceDesignMaskDesignUid}");
          }
          */
        }
      }

      return result;
    }

    /// <summary>
    /// Prepare a set of filter for use by performing any necessary coordinate conversions and requesting any
    /// supplemental information such as alignment design boundary calculations.
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="dataModelId"></param>
    /// <returns></returns>
    public static async Task<RequestErrorStatus> PrepareFiltersForUse(ICombinedFilter[] filters, Guid dataModelId)
    {
      var status = RequestErrorStatus.Unknown;

      foreach (var filter in filters)
      {
        if (filter != null)
        {
          status = await PrepareFilterForUse(filter, dataModelId);

          if (status != RequestErrorStatus.OK)
            break;
        }
      }

      return status;
    }
  }
}

using System;
using CoreX.Interfaces;
using CoreX.Types;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Filters
{
  public static class FilterUtilities
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger(nameof(FilterUtilities));

    /*
    private IExistenceMaps existenceMaps = null;
    private IExistenceMaps GetExistenceMaps() => existenceMaps ??= DIContext.Obtain<IExistenceMaps>());
    */

    /// <summary>
    /// Prepare a filter for use by performing any necessary coordinate conversions and requesting any
    /// supplemental information such as alignment design boundary calculations.
    /// </summary>
    public static RequestErrorStatus PrepareFilterForUse(ICombinedFilter filter, Guid dataModelId)
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
          var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(dataModelId);

          XYZ[] llhCoords;
          // If the filter has a spatial or positional context, then convert the LLH values in the
          // spatial context into the NEE values consistent with the data model.
          if (filter.SpatialFilter.IsSpatial)
          {
            llhCoords = new XYZ[filter.SpatialFilter.Fence.NumVertices];

            // Note: Lat/Lons in filter fence boundaries are supplied to us in decimal degrees, not radians
            for (var fencePointIdx = 0; fencePointIdx < filter.SpatialFilter.Fence.NumVertices; fencePointIdx++)
            {
              llhCoords[fencePointIdx] = new XYZ(MathUtilities.DegreesToRadians(filter.SpatialFilter.Fence[fencePointIdx].X), MathUtilities.DegreesToRadians(filter.SpatialFilter.Fence[fencePointIdx].Y), 0);
            }

            XYZ[] neeCoords;

            try
            {
              neeCoords = DIContext
              .Obtain<ICoreXWrapper>()
              .LLHToNEE(siteModel.CSIB(), llhCoords.ToCoreX_XYZ(), InputAs.Radians)
              .ToTRex_XYZ();
            }
            catch
            {
              return RequestErrorStatus.FailedToConvertClientWGSCoords;
            }


            for (var fencePointIdx = 0; fencePointIdx < filter.SpatialFilter.Fence.NumVertices; fencePointIdx++)
            {
              filter.SpatialFilter.Fence[fencePointIdx].X = neeCoords[fencePointIdx].X;
              filter.SpatialFilter.Fence[fencePointIdx].Y = neeCoords[fencePointIdx].Y;
            }

            // Ensure that the bounding rectangle for the filter fence correctly encloses the newly calculated grid coordinates
            filter.SpatialFilter.Fence.UpdateExtents();
          }

          if (filter.SpatialFilter.IsPositional)
          {
            // Note: Lat/Lons in positions are supplied to us in decimal degrees, not radians
            llhCoords = new[] { new XYZ(MathUtilities.DegreesToRadians(filter.SpatialFilter.PositionX), MathUtilities.DegreesToRadians(filter.SpatialFilter.PositionY), 0) };

            XYZ[] neeCoords;

            try
            {
              neeCoords = DIContext
                .Obtain<ICoreXWrapper>()
                .LLHToNEE(siteModel.CSIB(), llhCoords.ToCoreX_XYZ(), InputAs.Radians)
                .ToTRex_XYZ();
            }
            catch
            {
              return RequestErrorStatus.FailedToConvertClientWGSCoords;
            }

            filter.SpatialFilter.PositionX = neeCoords[0].X;
            filter.SpatialFilter.PositionY = neeCoords[0].Y;
          }

          filter.SpatialFilter.CoordsAreGrid = true;
        }

        // Ensure that the bounding rectangle for the filter fence correctly encloses the newly calculated grid coordinates
        filter.SpatialFilter?.Fence.UpdateExtents();

        // Is there an alignment file to look up? If so, only do it if there not an already existing alignment fence boundary
        if (filter.SpatialFilter.HasAlignmentDesignMask() && !(filter.SpatialFilter.AlignmentFence?.HasVertices ?? true))
        {
          var boundaryRequest = new AlignmentDesignFilterBoundaryRequest();

          var boundaryResult = boundaryRequest.Execute
          (new AlignmentDesignFilterBoundaryArgument()
          {
            ProjectID = dataModelId,
            ReferenceDesign = new DesignOffset(filter.SpatialFilter.AlignmentDesignMaskDesignUID, 0),
            StartStation = filter.SpatialFilter.StartStation ?? Common.Consts.NullDouble,
            EndStation = filter.SpatialFilter.EndStation ?? Common.Consts.NullDouble,
            LeftOffset = filter.SpatialFilter.LeftOffset ?? Common.Consts.NullDouble,
            RightOffset = filter.SpatialFilter.RightOffset ?? Common.Consts.NullDouble,
            Filters = new FilterSet(new CombinedFilter())
          });

          if (boundaryResult.RequestResult != DesignProfilerRequestResult.OK)
          {
            _log.LogError($"{nameof(PrepareFilterForUse)}: Failed to get boundary for alignment design ID:{filter.SpatialFilter.AlignmentDesignMaskDesignUID}");

            return RequestErrorStatus.NoResultReturned;
          }

          filter.SpatialFilter.AlignmentFence = boundaryResult.Boundary;
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
    public static RequestErrorStatus PrepareFiltersForUse(ICombinedFilter[] filters, Guid dataModelId)
    {
      var status = RequestErrorStatus.Unknown;

      foreach (var filter in filters)
      {
        if (filter != null)
        {
          status = PrepareFilterForUse(filter, dataModelId);

          if (status != RequestErrorStatus.OK)
            break;
        }
      }

      return status;
    }

    /// <summary>
    /// Constructs the set of filters that will be used to derive the set of production data sub grids.
    /// </summary>
    /// <returns></returns>
    public static IFilterSet ConstructFilters(IFilterSet filterSet, VolumeComputationType volumeType)
    {
      if (volumeType == VolumeComputationType.None)
        return filterSet;

      // If the volume calculation is between two filters then handle appropriately...
      if (volumeType == VolumeComputationType.Between2Filters)
      {
        var baseFilter = filterSet.Filters[0];
        var topFilter = filterSet.Filters[1];

        // Determine if intermediary filter/surface behaviour is required to support summary volumes
        var intermediaryFilterRequired = volumeType == VolumeComputationType.Between2Filters &&
                                         baseFilter.AttributeFilter.HasTimeFilter && baseFilter.AttributeFilter.StartTime == Consts.MIN_DATETIME_AS_UTC && // 'From' has As-At Time filter
                                          !baseFilter.AttributeFilter.ReturnEarliestFilteredCellPass && // Want latest cell pass in 'from'
                                         topFilter.AttributeFilter.HasTimeFilter && topFilter.AttributeFilter.StartTime != Consts.MIN_DATETIME_AS_UTC && // 'To' has time-range filter with latest
                                          !topFilter.AttributeFilter.ReturnEarliestFilteredCellPass; // Want latest cell pass in 'to'

        if (intermediaryFilterRequired)
        {
          // Create and use the intermediary filter. The intermediary filter
          // is created from the Top filter, with the return earliest flag set to true
          var intermediaryFilter = new CombinedFilter();
          intermediaryFilter.AttributeFilter.Assign(topFilter.AttributeFilter);
          intermediaryFilter.AttributeFilter.ReturnEarliestFilteredCellPass = true;
          intermediaryFilter.SpatialFilter.Assign(topFilter.SpatialFilter);

          return new FilterSet(new[] { baseFilter, intermediaryFilter, topFilter });
        }
      }

      else if (volumeType == VolumeComputationType.BetweenDesignAndFilter)
        return new FilterSet(filterSet.Filters[1]);

      else if (volumeType == VolumeComputationType.BetweenFilterAndDesign)
        return new FilterSet(filterSet.Filters[0]);

      return filterSet;
    }
  }
}

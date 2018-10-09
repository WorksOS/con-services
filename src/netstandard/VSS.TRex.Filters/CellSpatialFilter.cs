using System;
using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Utilities;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.Filters
{
  /*  Of the two varieties of filtering used, this unit supports:
      - Cell selection filtering

        Based on:
          Spatial: Arbitrary fence specifying inclusion area
          Positional: Point and radius for inclusion area

        The result of this filter is <YES> the cell may be used for cell pass
        filtering, or<NO> the cell should not be considered for cell pass
        filtering. 
  */

  /// <summary>
  /// CellSpatialFilter is a filter designed to filter cells for inclusion
  /// in the returned result. The aim of the filter is to say YES or NO to the inclusion
  /// of the cell. It does not choose which pass in the cell should be returned.
  /// 
  /// Of the two varieties of filtering used, this unit supports:
  /// - Cell selection filtering
  ///
  ///   Based on:
  ///     Spatial: Arbitrary fence specifying inclusion area
  ///     Positional: Point and radius for inclusion area
  ///
  ///   The result of this filter is YES the cell may be used for cell pass
  ///   filtering, or NO the cell should not be considered for cell pass filtering.
  /// </summary>
  public class CellSpatialFilter : CellSpatialFilterModel, ICellSpatialFilter
  {
 
        /// <summary>
        ///  Spatial cell filter constructor
        /// </summary>
        public CellSpatialFilter()
        {
          Clear();
        }

        public CellSpatialFilter(IBinaryRawReader reader)
        {
            FromBinary(reader);
        }

        /// <summary>
        /// Return a formatted string indicating the state of the filter flags
        /// </summary>
        /// <returns></returns>
        public string ActiveFiltersString()
        {
            return $"Spatial:{IsSpatial}, Positional:{IsPositional}, DesignMask:{IsDesignMask}, AlignmentMask:{IsAlignmentMask}";
        }

        /// <summary>
        /// Clears all filter state to a state that will pass (accept) all cells
        /// </summary>
        public void Clear()
        {
            ClearPositional();
            ClearSpatial();
            ClearDesignMask();
            ClearAlignmentMask();
        }

        /// <summary>
        /// Removes all state related to an alignment mask filter and sets the alignment mask type to off
        /// </summary>
        public void ClearAlignmentMask()
        {
            IsAlignmentMask = false;

            AlignmentFence.Clear();
            StartStation = null;
            EndStation = null;
            LeftOffset = null;
            RightOffset = null;

            AlignmentMaskDesignUID = Guid.Empty;
        }

        /// <summary>
        /// Removes all state related to an design mask filter and sets the design mask type to off
        /// </summary>
        public void ClearDesignMask()
        {
            IsDesignMask = false;

            SurfaceDesignMaskDesignUid = Guid.Empty;
        }

        /// <summary>
        /// Removes all state related to a positional filter and sets the positional mask type to off
        /// </summary>
        public void ClearPositional()
        {
            IsPositional = false;

            PositionX = Consts.NullDouble;
            PositionY = Consts.NullDouble;
            PositionRadius = Consts.NullDouble;
            IsSquare = false;
        }

        /// <summary>
        /// Removes all state related to a polygonal filter and sets the spatial mask type to off
        /// </summary>
        public void ClearSpatial()
        {
            IsSpatial = false;

            Fence.Clear();
        }

        /// <summary>
        /// Determines if the filter contains sufficient information to adequately describe an active alignment
        /// or design mask spatial filter
        /// </summary>
        /// <returns></returns>
        public bool HasAlignmentDesignMask()
        {
            return AlignmentMaskDesignUID != Guid.Empty && 
                   StartStation.HasValue && EndStation.HasValue &&
                   LeftOffset.HasValue && RightOffset.HasValue;
        }

        /// <summary>
        /// Determines if the filter contains sufficient information to adequately describe an active alignment
        /// or design mask spatial filter
        /// </summary>
        /// <returns></returns>
        public bool HasSurfaceDesignMask => SurfaceDesignMaskDesignUid != Guid.Empty;

        /// <summary>
        /// Determines if the type of the spatial filter is Spatial or Positional
        /// </summary>
        public bool HasSpatialOrPostionalFilters => IsSpatial || IsPositional || IsDesignMask || IsAlignmentMask;

        /// <summary>
        /// Determines if a cell given by it's central location is included in the spatial filter
        /// </summary>
        /// <param name="CellCenterX"></param>
        /// <param name="CellCenterY"></param>
        /// <returns></returns>
        public bool IsCellInSelection(double CellCenterX, double CellCenterY)
        {
            if (IsSpatial)
            {
                return Fence.IncludesPoint(CellCenterX, CellCenterY);
            }

            if (IsPositional)
            {
                if (IsSquare)
                {
                    return !(CellCenterX < PositionX - PositionRadius ||
                             CellCenterX > PositionX + PositionRadius ||
                             CellCenterY < PositionY - PositionRadius ||
                             CellCenterY > PositionY + PositionRadius);
                }

                double Distance = MathUtilities.Hypot(CellCenterX - PositionX, CellCenterY - PositionY);
                return Distance < PositionRadius;
            }

            return true;
        }

        /// <summary>
        /// Determines if an arbitrary location is included in the spatial filter.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public bool IsPositionInSelection(double X, double Y) => IsCellInSelection(X, Y);

        /// <summary>
        /// Calculate a bounding extent of this spatial filter with a given external bounding extent
        /// </summary>
        /// <param name="Extents"></param>
        /// <returns></returns>
        public BoundingWorldExtent3D CalculateIntersectionWithExtents(BoundingWorldExtent3D Extents)
        {
            if (IsSpatial) // Just a polygonal fence
            {
                Fence.GetExtents(out double MinX, out double MinY, out double MaxX, out double MaxY);
                return Extents.Intersect(MinX, MinY, MaxX, MaxY);
            }

            if (IsPositional) // Square or circle
            {
                return Extents.Intersect(PositionX - PositionRadius,
                    PositionY - PositionRadius,
                    PositionX + PositionRadius,
                    PositionY + PositionRadius);
            }

            // no spatial restriction in the filter
            return Extents;
        }
    }
}

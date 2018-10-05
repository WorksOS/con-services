using System;
using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Interfaces;
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
    public class CellSpatialFilter : ICellSpatialFilter, IToFromBinary
  {
        /// <summary>
        /// The fence used for polygon based spatial filtering
        /// </summary>
        public Fence Fence { get; set; } = new Fence();

        /// <summary>
        /// The fence used to represent the spatial restriction derived from an alignment filter expressed as a 
        /// station and offset range with respect tot he alignment center line geometry expressed as a polygon
        /// </summary>
        public Fence AlignmentFence { get; set; } = new Fence(); // contains alignment boundary to help speed up filtering on alignment files

        // Positional based filtering

        /// <summary>
        /// The X ordinate of the positional spatial filter
        /// </summary>
        public double PositionX { get; set; } = Consts.NullDouble;

        /// <summary>
        /// The Y ordinate of the positional spatial filter
        /// </summary>
        public double PositionY { get; set; } = Consts.NullDouble;

        /// <summary>
        /// The radius of the positional spatial filter for point-radius positional filters
        /// </summary>
        public double PositionRadius { get; set; } = Consts.NullDouble;

        /// <summary>
        /// Determines if the point-radius should be applied as a square rather than a circle
        /// </summary>
        public bool IsSquare { get; set; }

        /// <summary>
        /// OverrideSpatialCellRestriction provides a rectangular, cell address based,
        /// restrictive boundary that overrides all other cell selection filter considerations
        /// in that it is always evaluated first. This is useful in contexts such as Web Map Service
        /// tile generation where the tile region itself is an overriding constraint
        /// on the data that needs to be queried
        /// </summary>
        public BoundingIntegerExtent2D OverrideSpatialCellRestriction { get; set; } = new BoundingIntegerExtent2D();

      // <summary>
      // A design that acts as a spatial filter for cell selection. Only cells that have center locations that lie over the 
      // design recorded in DesignFilter will be included
      // </summary>
      //    public Guid DesignFilterUID = Guid.Empty;
      //        public DesignDescriptor DesignFilter = DesignDescriptor.Null(); 

        /// <summary>
        /// The starting station of the parametrically defined alignment spatial filter
        /// </summary>
        public double? StartStation { get; set; }

        /// <summary>
        /// The ending station of the parametrically defined alignment spatial filter
        /// </summary>
        public double? EndStation { get; set; }

        /// <summary>
        /// The left offset of the parametrically defined alignment spatial filter
        /// </summary>
        public double? LeftOffset { get; set; }

        /// <summary>
        /// The right offset of the parametrically defined alignment spatial filter
        /// </summary>
        public double? RightOffset { get; set; }

        /// <summary>
        /// CoordsAreGrid controls whether the plan (XY/NE) coordinates in the spatial filters are to 
        /// be interpreted as rectangular cartesian coordinates or as WGS84 latitude/longitude coordinates
        /// </summary>
        public bool CoordsAreGrid { get; set; }

        /// <summary>
        /// Restricts cells to spatial fence
        /// </summary>
        public bool IsSpatial { get; set; }

        /// <summary>
        /// Restricts cells to spatial fence
        /// </summary>
        public bool IsPositional { get; set; }

        /// <summary>
        /// Using a loaded surface design to 'mask' the cells that should be included in the filter
        /// </summary>
        public bool IsDesignMask { get; set; }

        /// <summary>
        /// A design that acts as a spatial filter for cell selection. Only cells that have center locations that lie over the 
        /// design recorded in DesignMask will be included
        /// </summary>
        public Guid SurfaceDesignMaskDesignUid { get; set; } = Guid.Empty;

        /// <summary>
        /// Using a load alignment design to 'mask' the cells that should be included in the filter
        /// </summary>
        public bool IsAlignmentMask { get; set; }

        /// <summary>
        /// The design used as an alignment mask spatial filter
        /// </summary>
        public Guid AlignmentMaskDesignUID { get; set; } = Guid.Empty;

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

    /// <summary>
    /// Serialize out the state of the cell spatial filter using the Ignite binarizable serialisation
    /// </summary>
    /// <param name="writer"></param>
    public void ToBinary(IBinaryRawWriter writer)
    {
      const byte versionNumber = 1;

      writer.WriteByte(versionNumber);

      writer.WriteBoolean(Fence != null);
      Fence?.ToBinary(writer);

      writer.WriteBoolean(AlignmentFence != null);
      AlignmentFence?.ToBinary(writer);

      writer.WriteDouble(PositionX);
      writer.WriteDouble(PositionY);
      writer.WriteDouble(PositionRadius);
      writer.WriteBoolean(IsSquare);

      OverrideSpatialCellRestriction.ToBinary(writer);

      writer.WriteBoolean(StartStation.HasValue);
      if (StartStation.HasValue)
        writer.WriteDouble(StartStation.Value);

      writer.WriteBoolean(EndStation.HasValue);
      if (EndStation.HasValue)
        writer.WriteDouble(EndStation.Value);

      writer.WriteBoolean(LeftOffset.HasValue);
      if (LeftOffset.HasValue)
        writer.WriteDouble(LeftOffset.Value);

      writer.WriteBoolean(RightOffset.HasValue);
      if (RightOffset.HasValue)
        writer.WriteDouble(RightOffset.Value);

      writer.WriteBoolean(CoordsAreGrid);
      writer.WriteBoolean(IsSpatial);
      writer.WriteBoolean(IsPositional);

      writer.WriteBoolean(IsDesignMask);
      writer.WriteGuid(SurfaceDesignMaskDesignUid);

      writer.WriteBoolean(IsAlignmentMask);
      writer.WriteGuid(AlignmentMaskDesignUID);
    }

    /// <summary>
    /// Serialize in the state of the cell spatial filter using the Ignite binarizable serialisation
    /// </summary>
    /// <param name="reader"></param>
    public void FromBinary(IBinaryRawReader reader)
    {
      const byte versionNumber = 1;
      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      if (reader.ReadBoolean())
        (Fence ?? new Fence()).FromBinary(reader);

      if (reader.ReadBoolean())
        (AlignmentFence ?? new Fence()).FromBinary(reader);

      PositionX = reader.ReadDouble();
      PositionY = reader.ReadDouble();
      PositionRadius = reader.ReadDouble();

      OverrideSpatialCellRestriction.FromBinary(reader);

      StartStation = reader.ReadBoolean() ? reader.ReadDouble() : (double?)null;
      EndStation = reader.ReadBoolean() ? reader.ReadDouble() : (double?)null;
      LeftOffset = reader.ReadBoolean() ? reader.ReadDouble() : (double?)null;
      RightOffset = reader.ReadBoolean() ? reader.ReadDouble() : (double?)null;

      CoordsAreGrid = reader.ReadBoolean();
      IsSpatial = reader.ReadBoolean();
      IsPositional = reader.ReadBoolean();

      IsDesignMask = reader.ReadBoolean();
      SurfaceDesignMaskDesignUid = reader.ReadGuid() ?? Guid.Empty;
      IsAlignmentMask = reader.ReadBoolean();
      AlignmentMaskDesignUID = reader.ReadGuid() ?? Guid.Empty;
    }
  }
}

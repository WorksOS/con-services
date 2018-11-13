using System;
using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// Combined filter represents both spatial and attribute based filtering considerations
  /// </summary>
  public class CombinedFilter : ICombinedFilter, IEquatable<ICombinedFilter>
  {
/// <summary>
    /// The filter responsible for selection of cell passes based on attribute filtering criteria related to cell passes
    /// </summary>
    public ICellPassAttributeFilter AttributeFilter { get; set; }

    /// <summary>
    /// The filter responsible for selection of cells based on spatial filtering criteria related to cell location
    /// </summary>
    public ICellSpatialFilter SpatialFilter { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CombinedFilter()
    {
      AttributeFilter = new CellPassAttributeFilter();
      SpatialFilter = new CellSpatialFilter();
    }

    /// <summary>
    /// Creates a new combined filter based on a supplied spatial filter and a newly created
    /// attribute filter
    /// </summary>
    /// <param name="spatialFilter"></param>
    public CombinedFilter(ICellSpatialFilter spatialFilter)
    {
      AttributeFilter = new CellPassAttributeFilter();
      SpatialFilter = spatialFilter;
    }

    public CombinedFilter(IBinaryRawReader reader)
    {
      FromBinary(reader);
    }

    /// <summary>
    /// Constructor accepting attribute and spatial filters
    /// </summary>
    /// <param name="attributeFilter"></param>
    /// <param name="spatialFilter"></param>
    public CombinedFilter(ICellPassAttributeFilter attributeFilter, ICellSpatialFilter spatialFilter)
    {
      AttributeFilter = attributeFilter;
      SpatialFilter = spatialFilter;
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      const byte versionNumber = 1;

      writer.WriteByte(versionNumber);

      writer.WriteBoolean(AttributeFilter != null);
      AttributeFilter?.ToBinary(writer);

      writer.WriteBoolean(SpatialFilter != null);
      SpatialFilter?.ToBinary(writer);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      const byte versionNumber = 1;
      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      if (reader.ReadBoolean())
        (AttributeFilter ?? (AttributeFilter = new CellPassAttributeFilter())).FromBinary(reader);

      if (reader.ReadBoolean())
        (SpatialFilter ?? (SpatialFilter = new CellSpatialFilter())).FromBinary(reader);
    }

    public bool Equals(ICombinedFilter other)
    {
      if (other == null)
        return false;

      return AttributeFilter.Equals(other.AttributeFilter) && SpatialFilter.Equals(other.SpatialFilter);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((ICombinedFilter) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return ((AttributeFilter != null ? AttributeFilter.GetHashCode() : 0) * 397) ^ (SpatialFilter != null ? SpatialFilter.GetHashCode() : 0);
      }
    }
  }
}

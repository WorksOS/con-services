using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// Combined filter represents both spatial and attribute based filtering considerations
  /// </summary>
  public class CombinedFilter : ICombinedFilter
  {
    private const byte versionNumber = 1;

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

    public CombinedFilter(IBinaryRawReader reader)
    {
      FromBinary(reader);
    }

    /// <summary>
    /// Constructor accepting attribute and spatial filters
    /// </summary>
    /// <param name="attributeFilter"></param>
    /// <param name="spatialFilter"></param>
    public CombinedFilter(ICellPassAttributeFilter attributeFilter, ICellSpatialFilter spatialFilter) : this()
    {
      AttributeFilter = attributeFilter;
      SpatialFilter = spatialFilter;
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteBoolean(AttributeFilter != null);
      AttributeFilter?.ToBinary(writer);

      writer.WriteBoolean(SpatialFilter != null);
      SpatialFilter?.ToBinary(writer);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      byte readVersionNumber = reader.ReadByte();

      Debug.Assert(readVersionNumber == versionNumber, $"Invalid version number: {readVersionNumber}, expecting {versionNumber}");

      if (reader.ReadBoolean())
        (AttributeFilter ?? new CellPassAttributeFilter()).FromBinary(reader);

      if (reader.ReadBoolean())
        (SpatialFilter ?? new CellSpatialFilter()).FromBinary(reader);

      throw new System.NotImplementedException();
    }
  }
}

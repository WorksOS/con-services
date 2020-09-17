using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// Combined filter represents both spatial and attribute based filtering considerations
  /// </summary>
  public class CombinedFilter : VersionCheckedBinarizableSerializationBase, ICombinedFilter
  {
    const byte VERSION_NUMBER = 1;

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
    ///  Handy helper function to make a configured filter
    /// </summary>
    public static CombinedFilter MakeFilterWith(Action<CombinedFilter> configure)
    {
      var combinedFilter = new CombinedFilter();
      configure(combinedFilter);
      return combinedFilter;
    }

    public CombinedFilter(IBinaryRawReader reader)
    {
      FromBinary(reader);
    }

    /// <summary>
    /// Constructor accepting attribute and spatial filters
    /// </summary>
    public CombinedFilter(ICellPassAttributeFilter attributeFilter, ICellSpatialFilter spatialFilter)
    {
      AttributeFilter = attributeFilter;
      SpatialFilter = spatialFilter;
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(AttributeFilter != null);
      AttributeFilter?.ToBinary(writer);

      writer.WriteBoolean(SpatialFilter != null);
      SpatialFilter?.ToBinary(writer);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        if (reader.ReadBoolean())
          (AttributeFilter ??= new CellPassAttributeFilter()).FromBinary(reader);

        if (reader.ReadBoolean())
          (SpatialFilter ??= new CellSpatialFilter()).FromBinary(reader);
      }
    }
  }
}

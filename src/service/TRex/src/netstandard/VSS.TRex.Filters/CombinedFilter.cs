﻿using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// Combined filter represents both spatial and attribute based filtering considerations
  /// </summary>
  public class CombinedFilter : ICombinedFilter
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
    /// <param name="configure"></param>
    /// <returns></returns>
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
    /// <param name="attributeFilter"></param>
    /// <param name="spatialFilter"></param>
    public CombinedFilter(ICellPassAttributeFilter attributeFilter, ICellSpatialFilter spatialFilter)
    {
      AttributeFilter = attributeFilter;
      SpatialFilter = spatialFilter;
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, CombinedFilter.VERSION_NUMBER);

      writer.WriteBoolean(AttributeFilter != null);
      AttributeFilter?.ToBinary(writer);

      writer.WriteBoolean(SpatialFilter != null);
      SpatialFilter?.ToBinary(writer);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, CombinedFilter.VERSION_NUMBER);

      if (reader.ReadBoolean())
        (AttributeFilter ?? (AttributeFilter = new CellPassAttributeFilter())).FromBinary(reader);

      if (reader.ReadBoolean())
        (SpatialFilter ?? (SpatialFilter = new CellSpatialFilter())).FromBinary(reader);
    }
  }
}

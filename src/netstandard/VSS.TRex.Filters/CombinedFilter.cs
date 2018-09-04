using System;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// Combined filter represents both spatial and attribute based filtering considerations
  /// </summary>
  [Serializable]
  public class CombinedFilter : ICombinedFilter
  {
    /// <summary>
    /// The filter reponsible for selection of cell passes based on attribute filtering criteria related to cell passes
    /// </summary>
    public ICellPassAttributeFilter AttributeFilter { get; set; }

    /// <summary>
    /// The filter responsible for selection of cells based on spatial filtering criteria related to cell location
    /// </summary>
    public ICellSpatialFilter SpatialFilter { get; set; }

    /// <summary>
    /// Defautl no-arg constructor
    /// </summary>
    public CombinedFilter()
    {
      AttributeFilter = new CellPassAttributeFilter();
      SpatialFilter = new CellSpatialFilter();
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
  }
}

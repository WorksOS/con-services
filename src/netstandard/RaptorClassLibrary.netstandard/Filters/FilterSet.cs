using System;
using VSS.TRex.Geometry;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// FilterSet represents a set of filters to be applied to each subgrid in a query within a single operation
  /// </summary>
  [Serializable]
  public class FilterSet
  {
    /// <summary>
    /// The list of combined attribute and spatial filters to be used
    /// </summary>
    public CombinedFilter[] Filters { get; set; }

    /// <summary>
    /// Default no-arg constructor that creates a zero-sized array of combined filters
    /// </summary>
    public FilterSet()
    {
      Filters = new CombinedFilter[0];
    }

    /// <summary>
    /// Constructor accepting a preinitialised array of filters to be included in the filter set
    /// </summary>
    /// <param name="filters"></param>
    public FilterSet(CombinedFilter[] filters)
    {
      Filters = filters;
    }

    /// <summary>
    /// Applies spatial filter restrictions to the extents required to request data for.
    /// </summary>
    /// <param name="extents"></param>
    public void ApplyFilterAndSubsetBoundariesToExtents(ref BoundingWorldExtent3D extents)
    {
      foreach (var filter in Filters)
        extents = filter?.SpatialFilter?.CalculateIntersectionWithExtents(extents) ?? extents;
    }
  }
}

using System;
using VSS.TRex.Geometry;
using VSS.TRex.Filters.Interfaces;

namespace VSS.TRex.Filters
{
  /// <summary>
  /// FilterSet represents a set of filters to be applied to each subgrid in a query within a single operation
  /// </summary>
  [Serializable]
  public class FilterSet : IFilterSet
  {
    /// <summary>
    /// The list of combined attribute and spatial filters to be used
    /// </summary>
    public ICombinedFilter[] Filters { get; set; }

    /// <summary>
    /// Default no-arg constructor that creates a zero-sized array of combined filters
    /// </summary>
    public FilterSet()
    {
      Filters = new ICombinedFilter[0];
    }

    /// <summary>
    /// Constructor accepting a single filters to be set into the filter set
    /// </summary>
    /// <param name="filter"></param>
    public FilterSet(ICombinedFilter filter)
    {
      Filters = new [] { filter };
    }

    /// <summary>
    /// Constructor accepting a preinitialised array of filters to be included in the filter set
    /// </summary>
    /// <param name="filters"></param>
    public FilterSet(ICombinedFilter[] filters)
    {
      Filters = filters;
    }

    /// <summary>
    /// Applies spatial filter restrictions to the extents required to request data for.
    /// </summary>
    /// <param name="extents"></param>
    public void ApplyFilterAndSubsetBoundariesToExtents(BoundingWorldExtent3D extents)
    {
      foreach (var filter in Filters)
      {
        filter?.SpatialFilter?.CalculateIntersectionWithExtents(extents);
      }
    }
  }
}

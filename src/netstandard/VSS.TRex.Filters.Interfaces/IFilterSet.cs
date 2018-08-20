using VSS.TRex.Geometry;

namespace VSS.TRex.Filters.Interfaces
{
  public interface IFilterSet
  {
    /// <summary>
    /// The list of combined attribute and spatial filters to be used
    /// </summary>
    ICombinedFilter[] Filters { get; set; }

    /// <summary>
    /// Applies spatial filter restrictions to the extents required to request data for.
    /// </summary>
    /// <param name="extents"></param>
    void ApplyFilterAndSubsetBoundariesToExtents(ref BoundingWorldExtent3D extents);
  }

}

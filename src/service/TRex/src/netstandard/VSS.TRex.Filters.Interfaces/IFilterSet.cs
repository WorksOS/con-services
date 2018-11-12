using VSS.TRex.Common.Interfaces;
using VSS.TRex.Geometry;

namespace VSS.TRex.Filters.Interfaces
{
  public interface IFilterSet : IFromToBinary
  {
    /// <summary>
    /// The list of combined attribute and spatial filters to be used
    /// </summary>
    ICombinedFilter[] Filters { get; set; }

    /// <summary>
    /// Applies spatial filter restrictions to the extents required to request data for.
    /// </summary>
    /// <param name="extents"></param>
    void ApplyFilterAndSubsetBoundariesToExtents(BoundingWorldExtent3D extents);
  }
}
